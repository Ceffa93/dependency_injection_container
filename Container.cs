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
        Debug.Assert(constrList.Length == 1, "A service must have a single constructor!");
        var constr = constrList.First();
        constructors[type] = constr;
    }

    public void construct()
    {
        var rootNodes = getRootNodes();
        services = new();

        foreach (var root in rootNodes)
        {
            buildGraph(root);
        }
    }

    public T get<T>()
    {
        return (T)services[typeof(T)];
    }

    private HashSet<Type> getRootNodes()
    {
        var rootNodes = new HashSet<Type>(constructors.Keys);
        foreach (var constr in constructors.Values)
            foreach (var param in constr.GetParameters())
                rootNodes.Remove(param.ParameterType);
        return rootNodes;
    }

    private void buildGraph(Type node)
    {
        List<object> args = new List<object>();
        var constr = constructors[node];

        foreach (var child in constr.GetParameters())
        {
            var paramType = child.ParameterType;
            if (!services.ContainsKey(paramType)) 
                buildGraph(paramType);
            args.Add(services[paramType]);
        }
        services[node] = constr.Invoke(args.ToArray());
        Console.WriteLine("Build " + node.FullName);
    }

    Dictionary<Type, ConstructorInfo> constructors;
    Dictionary<Type, object> services;
}
