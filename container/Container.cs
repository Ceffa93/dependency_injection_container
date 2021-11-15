namespace DI
{
    using ConstructorDict = Dictionary<Type, Constructor>;

    public class Container : IDisposable
    {
        #region public

        public Container(ServiceList serviceList)
        {
            initOrder = new();
            services = new();
            implementDict = new();

            GenerateImplementDict(serviceList.serviceDescriptors);

            AddExternalServices(serviceList.externalServices);
            AddInternalServices(serviceList.internalServices, serviceList.serviceDescriptors);
        }

        public T Get<T>()
          where T : class
        {
            if (bIsDisposed)
                throw new ObjectDisposedException("Container has been disposed!");

            var type = typeof(T);

            if (!services.ContainsKey(type))
                throw new ContainerException("Service <" + type.Name + "> not found!");

            return (T)services[type];
        }
        public void Get<T>(out T[] implementServices)
          where T : class
        {
            if (bIsDisposed)
                throw new ObjectDisposedException("Container has been disposed!");

            var type = typeof(T);

            if (!implementDict.ContainsKey(type))
                throw new ContainerException("No service implements <" + type.Name + ">!");

            var implements = implementDict[type];

            var serviceList = new List<T>(implements.Count);

            foreach (var serviceType in implements)
                serviceList.Add((T)services[serviceType]);
               
            implementServices = serviceList.ToArray();
        }

        #endregion

        #region private

        private void AddExternalServices(ObjectDict externalServices)
        {
            foreach (var service in externalServices)
                services[service.Key] = service.Value;
        }

        private void AddInternalServices(TypeSet internalServices, ServiceDescDict serviceDescriptors)
        {
            var constructorDict = GenerateConstructorDict(internalServices, serviceDescriptors);

            TypeStack typeStack = new(constructorDict.Count);

            foreach (var type in FindRootTypes(constructorDict))
                typeStack.Push(type);

            while (typeStack.Count > 0)
                ProcessStack(constructorDict, typeStack);
        }

        private ConstructorDict GenerateConstructorDict(TypeSet internalServices, ServiceDescDict serviceDescriptors)
        {
            var constructorDict = new ConstructorDict();

            foreach (var type in internalServices)
                constructorDict[type] = new (type, serviceDescriptors, implementDict);

            return constructorDict;
        }

        private void GenerateImplementDict(ServiceDescDict serviceDescriptors)
        {
            foreach (var desc in serviceDescriptors.Values)
                desc.AddImplementsToDict(implementDict);
        }

        private static TypeSet FindRootTypes(ConstructorDict constructorDict)
        {
            TypeSet rootTypes = new(constructorDict.Keys);

            foreach (var constr in constructorDict.Values)
                constr.RemoveParamsFromSet(rootTypes);

            return rootTypes;
        }

        private void ProcessStack(ConstructorDict constructorDict, TypeStack typeStack)
        {
            var type = typeStack.Peek();
            var constructor = constructorDict[type];

            if (constructor.TryAddParamsToStack(typeStack, services)) 
                return;

            typeStack.Pop();
            initOrder.Add(type);
            services[type] = constructor.Construct(services);
        }
       
        private readonly ObjectDict services;
        private readonly TypeList initOrder;
        private readonly ImplementDict implementDict;

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
}
