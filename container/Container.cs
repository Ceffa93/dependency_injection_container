using System.Reflection;

public class ServiceList
{
    public ServiceList()
    {
        externalServices = new();
        requestedServices = new();
    }
    public void Add<T>(T externalService)
       where T : class
    {
        Type type = typeof(T);

        serviceAlreadyPresentException(type);
        
        if (externalService is null)
            throw new ContainerException("External service <" + type + "> is null!");

        externalServices[type] = externalService;
    }

    public void Add<T>()
        where T : class
    {
        Type type = typeof(T);
        
        serviceAlreadyPresentException(type);

        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        if (constrList.Length > 1)
            throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

        requestedServices[type] = constrList.First();
    }

    private void serviceAlreadyPresentException(Type type)
    {
        if (requestedServices.ContainsKey(type) || externalServices.ContainsKey(type))
            throw new ContainerException("Requested service <" + type + "> is already present!");
    }

    internal readonly Dictionary<Type, ConstructorInfo> requestedServices;
    internal readonly Dictionary<Type, object> externalServices;
}

public class Container : IDisposable
{
    public Container(ServiceList serviceList)
    {
        initOrder = new();
        services = new(serviceList.externalServices);

        ConstructRequestedServices(serviceList.requestedServices);
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

    private void ConstructRequestedServices(Dictionary<Type, ConstructorInfo> requestedServices)
    {
        Stack<Type> stack = new(requestedServices.Count);

        foreach (var type in FindRootTypes(requestedServices))
            stack.Push(type);

        while (stack.Count > 0)
            if(!TryAddChildrenToStack(requestedServices, stack, stack.Peek()))
                CreateService(requestedServices, stack.Pop());
    }

    private HashSet<Type> FindRootTypes(Dictionary<Type, ConstructorInfo> requestedServices)
    {
        HashSet<Type> rootTypes = new (requestedServices.Keys);

        foreach (var constr in requestedServices.Values)
            foreach (var param in constr.GetParameters())
                rootTypes.Remove(param.ParameterType);

        return rootTypes;
    }

    private bool TryAddChildrenToStack(Dictionary<Type, ConstructorInfo> requestedServices, Stack<Type> stack, Type type)
    {
        bool bAddedChildren = false;

        foreach (var child in requestedServices[type].GetParameters())
            bAddedChildren |= TryAddToStack(requestedServices, stack, child.ParameterType, type);

        return bAddedChildren;
    }

    private bool TryAddToStack(Dictionary<Type, ConstructorInfo> requestedServices, Stack<Type> stack, Type type, Type parentType)
    {
       if (services.ContainsKey(type))
            return false;

        if (!requestedServices.ContainsKey(type))
            throw new ContainerException("Dependency <" + type + "> required by <" + parentType + "> is missing!");

        stack.Push(type);
        return true;
    }

    private void CreateService(Dictionary<Type, ConstructorInfo> requestedServices, Type type)
    {
        initOrder.Add(type);

        var parameters = requestedServices[type].GetParameters();
        var arguments = new List<object>(parameters.Length);

        foreach (var param in parameters)
            arguments.Add(services[param.ParameterType]);

        services[type] = requestedServices[type].Invoke(arguments.ToArray());
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

