// List of classes depending on each other to test the dependency injection container

class A { public A() { } }
class B { public B(E e, K k) { } }
class C { public C(G g, H h) { } }
class D { public D() { } }
class E { public E(K k) { } }
class F { public F(I i) { } }
class G { public G(J j, L l) { } }
class H { public H(L l, O o) { } }
class I { public I(N n) { } }
class J { public J(M m) { } }
class K { public K() { } }
class L { public L(M m) { } }
class M { public M() { } }
class N { public N(P p) { } }
class O { public O(P p) { } }
class P { public P(Q q) { } }
class Q{ public Q() { } }
class R { public R() { } }
