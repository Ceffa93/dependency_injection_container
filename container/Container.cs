using System.Reflection;

public class Container : IDisposable
{
    #region public

    public Container()
    {
        constructors = new();
        services = new();
        ownedServicesInitOrder = new();
    }

    public void Add<T>(T externalDependency) 
        where T : class
    {
        throwIfDisposed();

        Type type = typeof(T);

        if (externalDependency is null)
            throw new ContainerException("External dependency <" + type + "> is null!");

        services[type] = externalDependency;
    }

    public void Add<T>()
        where T : class
    {
        throwIfDisposed();

        Type type = typeof(T);

        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        if (constrList.Length > 1)
            throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

        constructors[type] = constrList.First();
    }

    public T Get<T>()
        where T : class
    {
        throwIfDisposed();

        var type = typeof(T);

        if (!services.ContainsKey(type))
            throw new ContainerException("Service <" + type + "> not found!");

        return (T) services[type];
    }

    public void Construct()
    {
        throwIfDisposed();

        Stack<Type> stack = new(constructors.Count);

        foreach (var type in FindRootTypes())
            stack.Push(type);

        while (stack.Count > 0)
            if(!TryAddChildrenToStack(stack, stack.Peek()))
                CreateService(stack.Pop());

        constructors.Clear();
    }
    #endregion

    #region private
    private HashSet<Type> FindRootTypes()
    {
        HashSet<Type> rootTypes = new (constructors.Keys);

        foreach (var constr in constructors.Values)
            foreach (var param in constr.GetParameters())
                rootTypes.Remove(param.ParameterType);

        return rootTypes;
    }

    private bool TryAddChildrenToStack(Stack<Type> stack, Type type)
    {
        bool bAddedChildren = false;

        foreach (var child in constructors[type].GetParameters())
            bAddedChildren |= TryAddToStack(stack, child.ParameterType, type);

        return bAddedChildren;
    }

    private bool TryAddToStack(Stack<Type> stack, Type type, Type parentType)
    {
       if (services.ContainsKey(type))
            return false;

        if (!constructors.ContainsKey(type))
            throw new ContainerException("Dependency <" + type + "> required by <" + parentType + "> is missing!");

        stack.Push(type);
        return true;
    }

    private void CreateService(Type type)
    {
        ownedServicesInitOrder.Add(type);

        var parameters = constructors[type].GetParameters();
        var arguments = new List<Object>(parameters.Length);

        foreach (var param in parameters)
            arguments.Add(services[param.ParameterType]);

        services[type] = constructors[type].Invoke(arguments.ToArray());
    }

    private Dictionary<Type, ConstructorInfo> constructors;
    private List<Type> ownedServicesInitOrder;
    private Dictionary<Type, Object> services;
    #endregion

    #region dispose

    public void Dispose()
    {
        if (!bIsDisposed)
            foreach (var type in ownedServicesInitOrder.AsEnumerable().Reverse())
                TryDispose(services[type]);

        bIsDisposed = true;
    }
    private void TryDispose(Object obj)
    {
        if (obj is IDisposable)
            ((IDisposable)obj).Dispose();
    }

    private void throwIfDisposed()
    {
        if(bIsDisposed)
            throw new ObjectDisposedException("Container has been disposed!");
    }

    private bool bIsDisposed;

    #endregion
}

public class ContainerException : Exception
{
    public ContainerException(string? message) : base(message) { }
};

