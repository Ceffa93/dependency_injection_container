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

            implementDict = GenerateImplementDict(serviceList.serviceDescriptors);

            AddExternalServices(serviceList.externalServices);
            AddInternalServices(serviceList.internalServices, serviceList.serviceDescriptors);
        }

        public T Get<T>()
          where T : class
        {
            CheckDisposed();
           
            var type = typeof(T);

            if (!services.ContainsKey(type))
                throw new ContainerException("Service <" + type.Name + "> not found!");

            return (T)services[type];
        }
        public void Get<T>(out T[] implementServices)
          where T : class
        {
            CheckDisposed();

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

        private static ImplementDict GenerateImplementDict(ServiceDescDict serviceDescriptors)
        {
            var implementDict = new ImplementDict();
            foreach (var desc in serviceDescriptors.Values)
                desc.AddImplementsToDict(implementDict);
            return implementDict;
        }

        private void AddExternalServices(ObjectDict externalServices)
        {
            foreach (var service in externalServices)
                services[service.Key] = service.Value;
        }

        private void AddInternalServices(TypeSet internalServices, ServiceDescDict serviceDescriptors)
        {
            var constructorDict = GenerateConstructorDict(internalServices, serviceDescriptors);
            var rootTypes = FindRootTypes(constructorDict);

            foreach (var type in rootTypes)
                Construct(constructorDict, type, ROOT, new());

            DetectDisconnectedCircularDependencies(constructorDict);
        }

        private ConstructorDict GenerateConstructorDict(TypeSet internalServices, ServiceDescDict serviceDescriptors)
        {
            var constructorDict = new ConstructorDict();

            foreach (var type in internalServices)
                constructorDict[type] = new (type, serviceDescriptors, implementDict);

            return constructorDict;
        }

        private static TypeSet FindRootTypes(ConstructorDict constructorDict)
        {
            TypeSet rootTypes = new(constructorDict.Keys);

            foreach (var constructor in constructorDict.Values)
                rootTypes.ExceptWith(constructor.GetDependencies());

            return rootTypes;
        }

        private void Construct(ConstructorDict constructorDict, Type type, Type parentType, TypeSet stack)
        {
            if (stack.Contains(type))
                throw new ContainerException("Circular dependency caused by <" + parentType.Name + "> including <" + type.Name + ">!");

            stack.Add(type);

            ConstructDependencies(constructorDict, type, stack);

            initOrder.Add(type);

            services[type] = constructorDict[type].Construct(services);

            stack.Remove(type);
        }

        private void ConstructDependencies(ConstructorDict constructorDict, Type type, TypeSet stack)
        {
            var allDeps = constructorDict[type].GetDependencies();
            var unresolvedDeps = allDeps.Where(depType => !services.ContainsKey(depType));

            foreach (var dep in unresolvedDeps)
                Construct(constructorDict, dep, type, stack);
        }

        private void DetectDisconnectedCircularDependencies(ConstructorDict constructorDict)
        {
            if (constructorDict.Count == initOrder.Count)
                return;

            var disconnectedTypes = constructorDict.Keys.Except(initOrder);

            string message = new("");
            foreach (var type in disconnectedTypes)
                message += "<" + type.Name + ">";
            
            throw new ContainerException("Disconnected circular detection detected on types " + message + ">!");
        }

        private readonly ObjectDict services;
        private readonly TypeList initOrder;
        private readonly ImplementDict implementDict;
        internal readonly static Type ROOT = typeof(void);

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
        private void CheckDisposed()
        {
            if (bIsDisposed)
                throw new ObjectDisposedException("Container has been disposed!");
        }

        private bool bIsDisposed;

        #endregion
    }
}
