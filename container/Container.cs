using System.Reflection;

namespace DIC
{
    using ImplementDict = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Type>>;
    using ConstructorDict = System.Collections.Generic.Dictionary<System.Type, Constructor>;

    public class Container : IDisposable
    {
        #region public

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

        #endregion

        #region private

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
                ProcessStack(constructorDict, stack);
        }

        private static ConstructorDict GenerateConstructorDict(ServiceList serviceList)
        {
            var implementDict = GenerateImplementDict(serviceList);

            ConstructorDict constructorDict = new();

            foreach (var type in serviceList.internalServices)
                constructorDict[type] = new (type, serviceList, implementDict);

            return constructorDict;
        }

        private static ImplementDict GenerateImplementDict(ServiceList serviceList)
        {
            ImplementDict implementDict = new();

            foreach (var child in serviceList.services)
                foreach (var parent in child.Value.implements)
                    AddToImplementDict(implementDict, child.Key, parent);

            return implementDict;
        }

        private static void AddToImplementDict(ImplementDict implementDict, Type child, Type parent)
        {
            if (implementDict.TryGetValue(parent, out var list))
                list.Add(child);
            else
                implementDict[parent] = new() { child };
        }


        private static HashSet<Type> FindRootTypes(ConstructorDict constructorDict)
        {
            HashSet<Type> rootTypes = new(constructorDict.Keys);

            foreach (var constr in constructorDict.Values)
                foreach (var param in constr.parameters)
                    rootTypes.Remove(param);

            return rootTypes;
        }

        private void ProcessStack(ConstructorDict constructorDict, Stack<Type> stack)
        {
            bool bAddedChildren = false;
            var type = stack.Peek();

            foreach (var childType in constructorDict[type].parameters)
                bAddedChildren |= TryAddToStack(stack, childType);

            if (bAddedChildren) 
                return;

            stack.Pop();
            CreateService(constructorDict[type], type);
        }

        private bool TryAddToStack(Stack<Type> stack, Type type)
        {
            if (services.ContainsKey(type))
                return false;

            stack.Push(type);
            return true;
        }

        private void CreateService(Constructor constructor, Type type)
        {
            initOrder.Add(type);

            var parameters = constructor.parameters;
            var arguments = new List<object>(parameters.Count);

            foreach (var param in parameters)
                arguments.Add(services[param]);

            services[type] = constructor.constrInfo.Invoke(arguments.ToArray());
        }

        private readonly Dictionary<Type, object> services;
        private readonly List<Type> initOrder;

        #endregion

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

            var constrParameters = constrInfo.GetParameters();

            parameters = new(constrParameters.Length);

            foreach (var param in constrParameters)
                parameters.Add(GetResolvedParam(serviceList, implementDict, param.ParameterType));
        }

        private static Type GetResolvedParam(ServiceList serviceList, ImplementDict implementDict, Type type)
        {
            if (serviceList.services.ContainsKey(type))
                return type;

            if (implementDict.TryGetValue(type, out var implementList))
                return implementList.First();

            throw new ContainerException("Parameter <" + type + "> required by <" + type + "> cannot be resolved!");
        }

        internal readonly ConstructorInfo constrInfo;
        internal readonly List<Type> parameters;
    };

    public class ContainerException : Exception
    {
        public ContainerException(string? message) : base(message) { }
    };

}
