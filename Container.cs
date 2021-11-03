using System.Reflection;
using System.Diagnostics;

class Container
{
    public Container()
    {
        constructors = new();
        services = new();
    }

    public void add<T>(T externalDependency) where T : notnull
    {
        services[typeof(T)] = externalDependency;
    }

    public void add<T>()
    {
        Type type = typeof(T);
        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        Debug.Assert(
            constrList.Length == 1, 
            "Services must have a single constructor!");

        constructors[type] = constrList.First();
    }

    public T get<T>()
    {
        var type = typeof(T);

        Debug.Assert(
            services.ContainsKey(type),
            "Service <" + type + "> not found!");

        return (T) services[type];
    }

    public void construct()
    {
        Stack<Type> stack = new(constructors.Count);

        foreach (var type in findRootTypes())
            stack.Push(type);

        while (stack.Count > 0)
            if(!tryAddChildrenToStack(stack, stack.Peek()))
                createService(stack.Pop());

        constructors.Clear();
    }

    private HashSet<Type> findRootTypes()
    {
        HashSet<Type> rootTypes = new (constructors.Keys);

        foreach (var constrInfo in constructors.Values)
            foreach (var paramInfo in constrInfo.GetParameters())
                rootTypes.Remove(paramInfo.ParameterType);

        return rootTypes;
    }

    private bool tryAddChildrenToStack(Stack<Type> stack, Type type)
    {
        bool bAddedDependencies = false;

        foreach (var childInfo in constructors[type].GetParameters())
            bAddedDependencies |= tryAddToStack(stack, childInfo.ParameterType, type);

        return bAddedDependencies;
    }

    private bool tryAddToStack(Stack<Type> stack, Type type, Type parentType)
    {
       if (services.ContainsKey(type))
            return false;

        Debug.Assert(
            constructors.ContainsKey(type),
            "Dependency <" + type + "> required by <" + parentType + "> is missing!");

        stack.Push(type);
        return true;
    }

    private void createService(Type type)
    {
        var parameters = constructors[type].GetParameters();
        var arguments = new List<object>(parameters.Length);

        foreach (var paramInfo in parameters)
            arguments.Add(services[paramInfo.ParameterType]);

        services[type] = constructors[type].Invoke(arguments.ToArray());
    }

    Dictionary<Type, ConstructorInfo> constructors;
    Dictionary<Type, object> services;
}
