using System.Reflection;

public abstract class ServiceInfo
{
    public ServiceInfo(Type type)
    {
        this.type = type;
        implements = new();
    }
    public ServiceInfo Implement<T>()
       where T : class
    {
        implements.Add(typeof(T));
        return this;
    }

    internal readonly HashSet<Type> implements;
    internal readonly Type type;
};

public class InternalServiceInfo : ServiceInfo
{
    public InternalServiceInfo(Type type) 
        : base(type)
    {
        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        if (constrList.Length > 1)
            throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

        constructorInfo = constrList.First();
    }

    internal readonly ConstructorInfo constructorInfo;
}
public class ExternalServiceInfo : ServiceInfo
{
    public ExternalServiceInfo(Type type, object obj) 
        : base(type)
    {
        if (obj is null)
            throw new ContainerException("External service <" + type + "> is null!");

        this.obj = obj;
    }

    internal readonly object obj;

}