class D { }
class C0 { public C0(D d) { } }
class C1 { }
class B0 { public B0(C1 c1, D d) { } }
class B1 { public B1(C0 c0) { } }
class A0 { public A0(B1 b1, C1 c1) { } }
class A1 { public A1(B0 b0) { } }