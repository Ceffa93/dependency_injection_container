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
        int count = constructors.Count;
        HashSet<Type> visited = new();
        Stack<Type> stack = new();
        List<Type> sorted = new();



        foreach (var root in getRootNodes())
        {
            stack.Push(root);
        }

        while(stack.Count > 0)
        {
            var node = stack.Peek();
            bool canCreate = true;
            foreach (var child in constructors[node].GetParameters())
            {
                if (!visited.Contains(child.ParameterType))
                {
                    canCreate = false;
                    stack.Push(child.ParameterType);
                }
            }
            if(canCreate)
            {
                visited.Add(node);
                sorted.Add(node);
                stack.Pop();
            }
        }

        services = new();

        foreach (var node in sorted)
        {
            List<object> args = new();
            foreach (var child in constructors[node].GetParameters())
            {
                Debug.Assert(services.ContainsKey(child.ParameterType));
                args.Add(services[child.ParameterType]);
            }
            services[node] = constructors[node].Invoke(args.ToArray());
            Console.WriteLine(node);
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

    Dictionary<Type, ConstructorInfo> constructors;
    Dictionary<Type, object> services;
}
