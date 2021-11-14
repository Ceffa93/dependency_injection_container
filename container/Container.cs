using System.Reflection;

namespace DIC
{
    using ImplementDict = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Type>>;
    using ConstructorDict = System.Collections.Generic.Dictionary<System.Type, Constructor>;

    public class Container : IDisposable
    {

        public Container(ServiceList serviceList)
        {
            initOrder = new();
            services = new();

            AddExternalServices(serviceList);
            AddInternalServices(serviceList);
        }

        public T Get<T>()
          where T : class
        {
            if (bIsDisposed)
                throw new ObjectDisposedException("Container has been disposed!");

            var type = typeof(T);

            if (!services.ContainsKey(type))
                throw new ContainerException("Service <" + type + "> not found!");

            return (T)services[type];
        }




        private void AddExternalServices(ServiceList serviceList)
        {
            foreach (var service in serviceList.externalServices)
                services[service.Key] = service.Value;
        }

        private void AddInternalServices(ServiceList serviceList)
        {
            var constructorDict = GenerateConstructorDict(serviceList);

            Stack<Type> stack = new(constructorDict.Count);

            foreach (var type in FindRootTypes(constructorDict))
                stack.Push(type);

            while (stack.Count > 0)
                if (!TryAddChildrenToStack(constructorDict, stack))
                    CreateService(constructorDict, stack);
        }

        private static ConstructorDict GenerateConstructorDict(ServiceList serviceList)
        {
            var implementDict = GenerateImplementDict(serviceList);

            ConstructorDict constructorDict = new();

            foreach (var type in serviceList.internalServices)
                constructorDict[type] =
                    new Constructor(
                        type,
                        serviceList,
                        implementDict);

            return constructorDict;
        }

        private static ImplementDict GenerateImplementDict(ServiceList serviceList)
        {
            ImplementDict implementInfos = new();

            foreach (var service in serviceList.services)
                foreach (var implement in service.Value.implements)
                    if (implementInfos.TryGetValue(implement, out var list))
                        list.Add(service.Key);
                    else
                        implementInfos[implement] = new() { service.Key };

            return implementInfos;
        }


        private static HashSet<Type> FindRootTypes(ConstructorDict constructorDict)
        {
            HashSet<Type> rootTypes = new(constructorDict.Keys);

            foreach (var constr in constructorDict.Values)
                foreach (var param in constr.parameters)
                    rootTypes.Remove(param);

            return rootTypes;
        }

        private bool TryAddChildrenToStack(ConstructorDict constructorDict, Stack<Type> stack)
        {
            bool bAddedChildren = false;

            foreach (var childType in constructorDict[stack.Peek()].parameters)
                bAddedChildren |= TryAddToStack(stack, childType);

            return bAddedChildren;
        }

        private bool TryAddToStack(Stack<Type> stack, Type type)
        {
            if (services.ContainsKey(type))
                return false;

            stack.Push(type);
            return true;
        }

        private void CreateService(ConstructorDict constructorDict, Stack<Type> stack)
        {
            var type = stack.Pop();
            var constructor = constructorDict[type];

            initOrder.Add(type);

            var parameters = constructor.parameters;
            var arguments = new List<object>(parameters.Count);

            foreach (var param in parameters)
                arguments.Add(services[param]);

            services[type] = constructor.constrInfo.Invoke(arguments.ToArray());
        }

        private readonly Dictionary<Type, object> services;
        private readonly List<Type> initOrder;

        #region dispose

        public void Dispose()
        {
            if (!bIsDisposed)
                foreach (var type in initOrder.AsEnumerable().Reverse())
                    TryDispose(services[type]);

            bIsDisposed = true;
        }
        private void TryDispose(object obj)
        {
            if (obj is IDisposable)
                ((IDisposable)obj).Dispose();
        }

        private bool bIsDisposed;

        #endregion
    }

    internal class Constructor
    {
        public Constructor(Type type, ServiceList serviceList, ImplementDict implementDict)
        {
            var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constrList.Length > 1)
                throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

            constrInfo = constrList.First();

            parameters = new();


            foreach (var param in constrInfo.GetParameters())
                if (serviceList.services.ContainsKey(param.ParameterType))
                    parameters.Add(param.ParameterType);
                else if (implementDict.TryGetValue(param.ParameterType, out var implementList) && implementList.Count == 1)
                    parameters.Add(implementList.First());
                else
                    throw new ContainerException("Parameter <" + param + "> required by <" + type + "> cannot be resolved!");
        }

        internal readonly ConstructorInfo constrInfo;
        internal readonly List<Type> parameters;
    };

    public class ContainerException : Exception
    {
        public ContainerException(string? message) : base(message) { }
    };

}
