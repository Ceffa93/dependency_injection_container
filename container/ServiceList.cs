namespace DI
{
    public class ServiceList
    {
        public ServiceList()
        {
            externalServices = new();
            internalServices = new();
            serviceDescriptors = new();
        }

        public ServiceDesc Add<T>(T externalService)
           where T : class
        {
            Type type = typeof(T);

            if (externalService is null)
                throw new ContainerException("External service <" + type.Name + "> is null!");

            AddExternalService(type, externalService);
            AddServiceDescriptor(type);

            return serviceDescriptors[type];
        }

        public ServiceDesc Add<T>()
            where T : class
        {
            Type type = typeof(T);

            AddInternalService(type);
            AddServiceDescriptor(type);

            return serviceDescriptors[type];
        }

        public void Add<T>(ServiceList sublist)
            where T : class
        {
            Type rootType = typeof(T);

            if(!sublist.serviceDescriptors.ContainsKey(rootType))
                throw new ContainerException("Sublist does not contain root service <" + rootType.Name + ">!");

            foreach (var service in sublist.externalServices)
                AddExternalService(service.Key, service.Value);

            foreach (var service in sublist.internalServices)
                AddInternalService(service);

            foreach (var service in sublist.serviceDescriptors.Values)
                AddSubListServiceDescriptor(rootType, service);
        }

        private void AddSubListServiceDescriptor(Type rootType, ServiceDesc subDesc)
        {
            if (serviceDescriptors.ContainsKey(subDesc.type))
                MergeSubListServiceDescriptor(rootType, subDesc);
            else
                NewSubListServiceDescriptor(rootType, subDesc);
        }

        private void MergeSubListServiceDescriptor(Type rootType, ServiceDesc subDesc)
        {
            var desc = serviceDescriptors[subDesc.type];
            desc.implements.Union(subDesc.implements);
            desc.roots.Add(rootType);
        }

        private void NewSubListServiceDescriptor(Type rootType, ServiceDesc subDesc)
        {
            var type = subDesc.type;
            var desc = serviceDescriptors[type] = subDesc;

            desc.roots.Add(rootType);
            if (type != rootType)
                desc.roots.Remove(ROOT);
        }

        private void AddExternalService(Type type, object service)
        {
            if (IsExternalServiceAlreadyPresent(type, service))
                return;

            externalServices[type] = service;

            if (internalServices.Contains(type))
                internalServices.Remove(type);
        }

        private void AddInternalService(Type type)
        {
            if (!serviceDescriptors.ContainsKey(type))
                internalServices.Add(type); 
        }

        private void AddServiceDescriptor(Type type)
        {
            if (!serviceDescriptors.ContainsKey(type))
                serviceDescriptors[type] = new(type);
            serviceDescriptors[type].roots.Add(ROOT);
        }

        private bool IsExternalServiceAlreadyPresent(Type type, object service)
        {
            if (!externalServices.TryGetValue(type, out var oldValue))
                return false;

            if (oldValue == service)
                return true;
               
            throw new ContainerException("Added two different external services of the same type <" + type.Name + ">!");
        }

        internal readonly ObjectDict externalServices;
        internal readonly TypeSet internalServices;
        internal readonly ServiceDescDict serviceDescriptors;
        internal readonly static Type ROOT = typeof(void);
    };
}
