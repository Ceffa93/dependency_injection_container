using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class SublistImplementTests
    {
        interface IInterface { }
        class X : IInterface { }
        class Y { }
        class Z { public Z(IInterface x) { } }


        [TestMethod]
        public void TestImplementInSublistVisible()
        {
            ServiceList childList = new();
            childList.Add<X>().Is<IInterface>();

            ServiceList list = new();
            list.Add<Z>();
            list.Add<X>(childList);
            
            new Container(list);
        }

        [TestMethod]
        public void TestImplementInSublistNotVisible()
        {
            ServiceList childList = new();
            childList.Add<X>().Is<IInterface>();
            childList.Add<Y>();

            ServiceList list = new();
            list.Add<Z>();
            list.Add<Y>(childList);

            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestMultipleRootsAndImplementDependenciesError()
        {
            ServiceList childList = new();
            childList.Add<X>().Is<IInterface>();
            childList.Add<Z>();

            ServiceList list = new();
            list.Add<Z>(childList);
            list.Add<X>().Is<IInterface>();
            list.Add<Z>();

            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestSameRootListsMerged()
        {
            ServiceList listA = new();
            listA.Add<X>().Is<IInterface>();
            listA.Add<Z>();

            ServiceList listB = new();
            listB.Add<X>().Is<IInterface>();
            listB.Add<Z>();

            ServiceList baseList = new();
            baseList.Add<X>(listA);
            baseList.Add<X>(listB);

            new Container(baseList);
        }
    }
}