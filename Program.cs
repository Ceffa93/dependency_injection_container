Container container = new Container();

// External dependencies
container.add(new Q());
container.add(new K());

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

container.construct();
