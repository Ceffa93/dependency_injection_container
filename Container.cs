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
        services = new List<object>();
    }

    public void add<T>() where T : new()
    {
        services.Add(new T());
    }

    public T get<T>()
    {
        return (T) services.First();
    }

    List<Object> services;
}
