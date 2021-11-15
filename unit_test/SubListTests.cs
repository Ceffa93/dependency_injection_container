using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class SubListTests
    {
        class A { }
        class B { public B(A x) { } }
        class C { public C(B x) { } }
        class D { public D(A x) { } }

        [TestMethod]
        public void TestCorrectScope()
        {
            ServiceList childList = new();
            childList.Add(new A());
            childList.Add<B>();

            ServiceList list = new();
            list.Add<C>();
            list.Add<B>(childList);

            new Container(list);
        }

        [TestMethod]
        public void TestWrongScope()
        {
            ServiceList childList = new();
            childList.Add(new A());
            childList.Add<B>();

            ServiceList list = new();
            list.Add<D>();
            list.Add<B>(childList);

            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDuplicatedInSublist()
        {
            ServiceList childList = new();
            childList.Add(new A());
            childList.Add<B>();

            ServiceList list = new();
            list.Add<A>();
            list.Add<B>(childList);
        }

        interface IInterface { }
        class X0 : IInterface { }
        class X1 : IInterface { }
        class X2 : IInterface { }
        class Y { public Y(X0 x){ } }
        class Z { public Z(IInterface[] x) { count = x.Length; } public int count; }

        [TestMethod]
        public void TestMulti()
        {
            ServiceList list0 = new();
            list0.Add<X0>().Is<IInterface>();
            list0.Add<Y>();

            ServiceList list1 = new();
            list1.Add<X1>().Is<IInterface>();

            ServiceList list = new();
            list.Add<X2>().Is<IInterface>();
            list.Add<Z>();
            list.Add<Y>(list0);
            list.Add<X1>(list1);

            var c = new Container(list);
            Assert.AreEqual(c.Get<Z>().count, 2);
        }
    }
}