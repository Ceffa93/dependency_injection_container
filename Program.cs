Container container = new Container();
container.add<A>();
container.construct();
A a = container.get<A>();
Console.WriteLine(a.ToString());


