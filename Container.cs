using System.Reflection;
using System.Diagnostics;

class Container
{
    public Container()
    {
        constructors = new();
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
        return (T)services[typeof(T)];
    }


    public void construct()
    {
        services = new(constructors.Count);

        Stack<Type> stack = new(constructors.Count);

        foreach (var root in findRootNodes())
            stack.Push(root);

        while (stack.Count > 0)
            if(!tryAddChildrenToStack(stack, stack.Peek()))
                createService(stack.Pop());

        constructors.Clear();
    }

    private HashSet<Type> findRootNodes()
    {
        HashSet<Type> rootNodes = new (constructors.Keys);
        foreach (var parentInfo in constructors)
            foreach (var paramInfo in parentInfo.Value.GetParameters())
                rootNodes.Remove(paramInfo.ParameterType);
        return rootNodes;
    }

    private bool tryAddChildrenToStack(Stack<Type> stack, Type parentType)
    {
        bool bAddedDependencies = false;
        foreach (var childInfo in constructors[parentType].GetParameters())
            bAddedDependencies |= tryAddToStack(stack, childInfo.ParameterType, parentType);
        return bAddedDependencies;
    }

    private bool tryAddToStack(Stack<Type> stack, Type type, Type parentType)
    {
        Debug.Assert(
            constructors.ContainsKey(type),
            "Dependency <" + type + "> required by <" + parentType + "> is missing!");

        if (services.ContainsKey(type))
            return false;

        stack.Push(type);
        return true;
    }

    private void createService(Type type)
    {
        var parameters = constructors[type].GetParameters();
        var arguments = new List<object>(parameters.Length);

        foreach (var childInfo in parameters)
            arguments.Add(services[childInfo.ParameterType]);

        services[type] = constructors[type].Invoke(arguments.ToArray());
        Console.WriteLine(type);
    }

    Dictionary<Type, ConstructorInfo> constructors;
    Dictionary<Type, object> services;
}
