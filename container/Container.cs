using System.Reflection;
using System.Diagnostics;

public class Container : IDisposable
{
    public Container()
    {
        constructors = new();
        services = new();
    }

    public void Add<T>(T externalDependency) where T : notnull
    {
        Type type = typeof(T);

        if (externalDependency is null)
            throw new ContainerException("External dependency <" + type + "> is null!");

        services[type] = externalDependency;
    }

    public void Add<T>()
    {
        Type type = typeof(T);

        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        if (constrList.Length == 0)
            throw new ContainerException("Class <" + type + "> must specify a public constructor!");

        if (constrList.Length > 1)
            throw new ContainerException("Class <" + type + "> must specify a single public constructor!");

        constructors[type] = constrList.First();
    }

    public T Get<T>()
    {
        var type = typeof(T);

        if (!services.ContainsKey(type))
            throw new ContainerException("Service <" + type + "> not found!");

        return (T) services[type];
    }

    public void Construct()
    {
        Stack<Type> stack = new(constructors.Count);
        ownedServicesInitOrder = new();

        foreach (var type in FindRootTypes())
            stack.Push(type);

        while (stack.Count > 0)
            if(!TryAddChildrenToStack(stack, stack.Peek()))
                CreateService(stack.Pop());

        constructors.Clear();
    }

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

    #region Dispose

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        if (!bIsDisposed)
            if (disposing)
                DisposeOwnedServices();

        bIsDisposed = true;
    }

    private void DisposeOwnedServices()
    {
        foreach (var type in ownedServicesInitOrder.AsEnumerable().Reverse())
            TryDispose(services[type]);
    }

    private void TryDispose(Object obj)
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

