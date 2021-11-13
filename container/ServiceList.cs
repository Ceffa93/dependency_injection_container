using System.Reflection;

public class ServiceList
{
    public ServiceList()
    {
        externalServiceInfos = new();
        internalServiceInfos = new();
    }
    public ServiceInfo Add<T>(T externalService)
       where T : class
    {
        Type type = typeof(T);

        serviceAlreadyPresentException(type);

        ExternalServiceInfo info = new(type, externalService);
        externalServiceInfos[type] = info;
        return info;
    }

    public ServiceInfo Add<T>()
        where T : class
    {
        Type type = typeof(T);
        
        serviceAlreadyPresentException(type);

        InternalServiceInfo info = new (type);
        internalServiceInfos[type] = info;
        return info;
    }

    private void serviceAlreadyPresentException(Type type)
    {
        if (internalServiceInfos.ContainsKey(type) || externalServiceInfos.ContainsKey(type))
            throw new ContainerException("Requested service <" + type + "> is already present!");
    }

    internal readonly Dictionary<Type, InternalServiceInfo> internalServiceInfos;
    internal readonly Dictionary<Type, ExternalServiceInfo> externalServiceInfos;
};

