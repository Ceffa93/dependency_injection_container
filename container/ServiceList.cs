namespace DIC
{
    public class ServiceList
    {
        public ServiceList()
        {
            externalServices = new();
            internalServices = new();
            services = new();
        }

        public Service Add<T>(T externalService)
           where T : class
        {
            Type type = typeof(T);

            serviceAlreadyPresentException(type);

            if (externalService is null)
                throw new ContainerException("External service <" + type + "> is null!");

            Service service = new(type);
            services[type] = service;
            externalServices[type] = externalService;
            return service;
        }

        public Service Add<T>()
            where T : class
        {
            Type type = typeof(T);

            serviceAlreadyPresentException(type);

            Service service = new(type);
            services[type] = service;
            internalServices.Add(type);
            return service;
        }

        private void serviceAlreadyPresentException(Type type)
        {
            if (services.ContainsKey(type))
                throw new ContainerException("Requested service <" + type + "> is already present!");
        }

        internal readonly Dictionary<Type, object> externalServices;
        internal readonly HashSet<Type> internalServices;
        internal readonly Dictionary<Type, Service> services;

        public class Service
        {
            public Service(Type type)
            {
                this.type = type;
                implements = new();
            }
            public Service Is<T>()
               where T : class
            {
                var parentType = typeof(T);

                if (parentType == type)
                    throw new ContainerException("Type <" + type + "> cannot implement itself!");

                if (!parentType.IsAssignableFrom(type))
                    throw new ContainerException("<" + parentType + "> is not a parent class/interface of <" + type + ">!");

                implements.Add(parentType);
                return this;
            }

            internal readonly HashSet<Type> implements;
            internal readonly Type type;
        };
    };
}
