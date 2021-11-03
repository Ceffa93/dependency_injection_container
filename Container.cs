using System.Reflection;
using System.Diagnostics;

class Container
{
    public Container()
    {
        constructors = new();
        services = new();
    }

    public void Add<T>(T externalDependency) where T : notnull
    {
        services[typeof(T)] = externalDependency;
    }

    public void Add<T>()
    {
        Type type = typeof(T);
        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        Debug.Assert(
            constrList.Length == 1, 
            "Services must have a single constructor!");

        constructors[type] = constrList.First();
    }

    public T Get<T>()
    {
        var type = typeof(T);

        Debug.Assert(
            services.ContainsKey(type),
            "Service <" + type + "> not found!");

        return (T) services[type];
    }

    public void Construct()
    {
        Stack<Type> stack = new(constructors.Count);

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

        foreach (var constrInfo in constructors.Values)
            foreach (var paramInfo in constrInfo.GetParameters())
                rootTypes.Remove(paramInfo.ParameterType);

        return rootTypes;
    }

    private bool TryAddChildrenToStack(Stack<Type> stack, Type type)
    {
        bool bAddedDependencies = false;

        foreach (var childInfo in constructors[type].GetParameters())
            bAddedDependencies |= TryAddToStack(stack, childInfo.ParameterType, type);

        return bAddedDependencies;
    }

    private bool TryAddToStack(Stack<Type> stack, Type type, Type parentType)
    {
       if (services.ContainsKey(type))
            return false;

        Debug.Assert(
            constructors.ContainsKey(type),
            "Dependency <" + type + "> required by <" + parentType + "> is missing!");

        stack.Push(type);
        return true;
    }

    private void CreateService(Type type)
    {
        var parameters = constructors[type].GetParameters();
        var arguments = new List<object>(parameters.Length);

        foreach (var paramInfo in parameters)
            arguments.Add(services[paramInfo.ParameterType]);

        services[type] = constructors[type].Invoke(arguments.ToArray());
    }

    private Dictionary<Type, ConstructorInfo> constructors;
    private Dictionary<Type, object> services;
}
