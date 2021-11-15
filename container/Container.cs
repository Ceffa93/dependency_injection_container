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

        private static ConstructorDict GenerateConstructorDict(TypeSet internalServices, ServiceDescDict serviceDescriptors)
        {
            var implementDict = GenerateImplementDict(serviceDescriptors);
            var constructorDict = new ConstructorDict();

            foreach (var type in internalServices)
                constructorDict[type] = new (type, serviceDescriptors, implementDict);

            return constructorDict;
        }

        private static ImplementDict GenerateImplementDict(ServiceDescDict serviceDescriptors)
        {
            ImplementDict implementDict = new();

            foreach (var desc in serviceDescriptors.Values)
                desc.AddImplementsToDict(implementDict);

            return implementDict;
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
