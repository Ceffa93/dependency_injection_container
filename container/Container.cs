using System.Reflection;

using ImplementInfos = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Type>>;

internal class Constructor
{
    public Constructor(ConstructorInfo constrInfo, List<Type> parameters)
    {
        this.constrInfo = constrInfo;
        this.parameters = parameters;
    }
    internal readonly ConstructorInfo constrInfo;
    internal readonly List<Type> parameters;
};

public class Container : IDisposable
{


    public Container(ServiceList serviceList)
    {
        initOrder = new();
        services = new();

        var internalServices = GetResolvedInternalServicesInfo(serviceList);
        AddExternalServices(serviceList);
        AddInternalServices(internalServices);
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

    private static Dictionary<Type, Constructor> GetResolvedInternalServicesInfo(ServiceList serviceList)
    {
        Dictionary<Type, Constructor> internalServices = new();
        var implementInfos = GenerateImplementInfos(serviceList);

        foreach (var service in serviceList.internalServiceInfos)
            internalServices[service.Key] = 
                new Constructor(       
                    service.Value.constructorInfo,
                    GetResolvedParameters(
                        service.Key,
                        serviceList,
                        implementInfos));

        return internalServices;
    }

    private static ImplementInfos GenerateImplementInfos(ServiceList serviceList)
    {
        ImplementInfos implementInfos = new();

        var serviceInfos = 
            serviceList.internalServiceInfos.Values.Cast<ServiceInfo>()
            .Concat(serviceList.externalServiceInfos.Values.Cast<ServiceInfo>());

        foreach (var service in serviceInfos)
            foreach (var implement in service.implements)
                if (implementInfos.TryGetValue(implement, out var list))
                    list.Add(service.type);
                else 
                    implementInfos[implement] = new() { service.type };

        return implementInfos;
    }

    private static List<Type> GetResolvedParameters(Type type, ServiceList serviceList, ImplementInfos implementInfos)
    {
        List<Type> resolvedParams = new();

        var parameters = serviceList.internalServiceInfos[type].constructorInfo.GetParameters();

        var serviceInfos =
            serviceList.internalServiceInfos.Keys
            .Concat(serviceList.externalServiceInfos.Keys);

        foreach (var param in parameters)
            if (serviceInfos.Contains(param.ParameterType)) 
                resolvedParams.Add(param.ParameterType);
            else if (implementInfos.TryGetValue(param.ParameterType, out var implementInfo) && implementInfo.Count == 1)
                resolvedParams.Add(implementInfo[0]);
            else
                throw new ContainerException("Parameter <" + param + "> required by <" + type + "> cannot be resolved!");

        return resolvedParams;
    }


    private void AddExternalServices(ServiceList serviceList)
    {
        foreach (var service in serviceList.externalServiceInfos.Values)
            services[service.type] = service.obj;
    }
    private void AddInternalServices(Dictionary<Type, Constructor> internalServiceInfos)
    {
        Stack<Type> stack = new(internalServiceInfos.Count);

        foreach (var type in FindRootTypes(internalServiceInfos))
            stack.Push(type);

        while (stack.Count > 0)
            if(!TryAddChildrenToStack(internalServiceInfos, stack, stack.Peek()))
                CreateService(internalServiceInfos[stack.Pop()]);
    }

    private HashSet<Type> FindRootTypes(Dictionary<Type, Constructor> internalServiceInfos)
    {
        HashSet<Type> rootTypes = new (internalServiceInfos.Keys);

        foreach (var info in internalServiceInfos.Values)
            foreach (var param in info.parameters)
                rootTypes.Remove(param);

        return rootTypes;
    }

    private bool TryAddChildrenToStack(Dictionary<Type, Constructor> internalServiceInfos, Stack<Type> stack, Type parentType)
    {
        bool bAddedChildren = false;

        foreach (var childType in internalServiceInfos[parentType].parameters)
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

    private void CreateService(Constructor constructor)
    {
        Type type = constructor.constrInfo.DeclaringType;
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

public class ContainerException : Exception
{
    public ContainerException(string? message) : base(message) { }
};

