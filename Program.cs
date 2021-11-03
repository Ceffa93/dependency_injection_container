// Create dependency injection container
Container container = new Container();

// Request services
container.Add<A>();
container.Add<B>();
container.Add<C>();
container.Add<D>();
container.Add<E>();
container.Add<F>();
container.Add<G>();
container.Add<H>();
container.Add<I>();
container.Add<J>();
container.Add<L>();
container.Add<M>();
container.Add<N>();
container.Add<O>();
container.Add<P>();
container.Add<R>();

// Add external dependencies
container.Add(new Q());
container.Add(new K());

// Create services
container.Construct();

// Get services
A a = container.Get<A>();
Q q = container.Get<Q>();
