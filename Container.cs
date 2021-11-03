using System.Reflection;
using System.Diagnostics;

class A
{
    public override string ToString()
    {
        return "A";
    }
}



class Container
{
    public Container()
    {
        constructors = new();
    }

    public void add<T>() where T : new()
    {
        Type type = typeof(T);
        var constrList = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
        Debug.Assert(constrList.Length == 1, "A service must have a single constructor!");
        var constr = constrList.First();
        constructors[type] = constr;
    }

    public void construct()
    {
        services = new();
        Type type = typeof(A);
        services[type] = constructors[typeof(A)].Invoke(null);
        constructors.Clear();
    }

    public T get<T>()
    {
        return (T)services[typeof(T)];
    }

    Dictionary<Type, ConstructorInfo> constructors;
    Dictionary<Type, object> services;
}
