// Create dependency injection container
Container container = new Container();

// Request services
container.add<A>();
container.add<B>();
container.add<C>();
container.add<D>();
container.add<E>();
container.add<F>();
container.add<G>();
container.add<H>();
container.add<I>();
container.add<J>();
container.add<L>();
container.add<M>();
container.add<N>();
container.add<O>();
container.add<P>();
container.add<R>();

// Add external dependencies
container.add(new Q());
container.add(new K());

// Create services
container.construct();

// Get services
A a = container.get<A>();
Q q = container.get<Q>();