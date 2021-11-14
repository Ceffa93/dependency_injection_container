using System.Reflection;

namespace DIC
{
    using ImplementDict = Dictionary<Type, List<Type>>;
    using ConstructorDict = Dictionary<Type, Constructor>;

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
                foreach (var desc in constr.paramDescriptors)
                    if (desc is ArrayParamDesc)
                        Array.ForEach(((ArrayParamDesc)desc).types, element => rootTypes.Remove(element));
                    else
                        rootTypes.Remove(((RegularParamDesc)desc).type);

            return rootTypes;
        }

        private void ProcessStack(ConstructorDict constructorDict, Stack<Type> stack)
        {
            bool bAddedChildren = false;
            var type = stack.Peek();

            foreach (var desc in constructorDict[type].paramDescriptors)
                if (desc is ArrayParamDesc)
                    Array.ForEach(((ArrayParamDesc)desc).types, element => bAddedChildren |= TryAddToStack(stack, element));
                else
                    bAddedChildren |= TryAddToStack(stack, ((RegularParamDesc)desc).type);

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

            var paramDescriptors = constructor.paramDescriptors;
            var arguments = new List<object>(paramDescriptors.Count);

            foreach (var desc in paramDescriptors)
                if (desc is ArrayParamDesc)
                {
                    var arrayDesc = (ArrayParamDesc)desc;
                    var types = arrayDesc.types;
                    var arg = Array.CreateInstance(arrayDesc.baseType, types.Length);

                    int idx = 0;
                    Array.ForEach(types, element => arg.SetValue(services[element], idx++));
                    arguments.Add(arg);
                }
                else
                    arguments.Add(services[((RegularParamDesc)desc).type]);

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

    internal abstract class ParamDesc { };

    internal class ArrayParamDesc: ParamDesc
    {
        public ArrayParamDesc(Type[] types, Type baseType)
        {
            this.types = types;
            this.baseType = baseType;
        }
        public Type[] types;
        public Type baseType;
    }

    internal class RegularParamDesc : ParamDesc
    {
        public RegularParamDesc(Type type) 
        { 
            this.type = type;
        }
        public Type type;
    }

    internal class Constructor
    {
        public Constructor(Type type, ServiceList serviceList, ImplementDict implementDict)
        {
            var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constrList.Length > 1)
                throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

            constrInfo = constrList.First();

            var parameters = constrInfo.GetParameters();

            paramDescriptors = new(parameters.Length);

            foreach (var param in parameters)
                if(param.ParameterType.IsArray)
                    paramDescriptors.Add(new ArrayParamDesc(GetResolvedArrayParam(serviceList, implementDict, param.ParameterType), param.ParameterType.GetElementType()));
                else
                    paramDescriptors.Add(new RegularParamDesc(GetResolvedRegularParam(serviceList, implementDict, param.ParameterType)));

        }

        private static Type[] GetResolvedArrayParam(ServiceList serviceList, ImplementDict implementDict, Type type)
        {
            var elementType = type.GetElementType();
            var implementList = implementDict.GetValueOrDefault(elementType);

            if (implementList is not null)
                return implementList.ToArray();

            throw new ContainerException("Array Parameter <" + type + "> cannot be resolved!");
        }

        private static Type GetResolvedRegularParam(ServiceList serviceList, ImplementDict implementDict, Type type)
        {
            if (serviceList.services.ContainsKey(type))
                return type;

            var implementList = implementDict.GetValueOrDefault(type);

            if (implementList is not null)
                return implementList.First();

            throw new ContainerException("Regular Parameter <" + type + "> cannot be resolved!");
        }

        internal readonly ConstructorInfo constrInfo;
        internal readonly List<ParamDesc> paramDescriptors;
    };

    public class ContainerException : Exception
    {
        public ContainerException(string? message) : base(message) { }
    };

}
