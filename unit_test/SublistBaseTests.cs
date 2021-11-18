using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class SublistTests
    {
        class A { }
        class B { public B(A x) { } }
        class C { public C(B x) { } }
        class D { public D(A x) { } }

        [TestMethod]
        public void TestInSublistVisible()
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
        public void TestInSublistNotVisible()
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
        public void TestMissingRoot()
        {
            ServiceList sublist = new();
            sublist.Add<A>();
            sublist.Add<B>();

            ServiceList list = new();
            try
            {
                list.Add<C>(sublist);
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

        [TestMethod]
        public void TestSublistDiamond()
        {
            ServiceList sharedList = new();
            sharedList.Add<A>();

            ServiceList list0 = new();
            list0.Add<A>(sharedList);
            list0.Add<B>();

            ServiceList list1 = new();
            list1.Add<A>(sharedList);
            list1.Add<D>();

            ServiceList baseList = new();
            baseList.Add<B>(list0);
            baseList.Add<D>(list1);

            new Container(baseList);
        }
    }
}