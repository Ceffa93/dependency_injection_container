using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class ArrayTests
    {
        interface IInterface { };
        class BaseClass { };

        class A0 : BaseClass { }
        class A1 : BaseClass, IInterface { }
        class B0 { public B0(BaseClass[] x) { count = x.Length; } public int count; }


        [TestMethod]
        public void TestArray()
        {
            ServiceList list = new();
            list.Add<A0>().Is<BaseClass>();
            list.Add<A1>().Is<BaseClass>();
            list.Add<B0>();
            Container container = new Container(list);
            Assert.AreEqual(container.Get<B0>().count, 2);
        }

        [TestMethod]
        public void TestEmptyArray()
        {
            ServiceList list = new();
            list.Add<B0>();
            Container container = new Container(list);
            Assert.AreEqual(container.Get<B0>().count, 0);
        }

        [TestMethod]
        public void TestSelf()
        {
            ServiceList list = new();
            try
            {
                list.Add<B0>().Is<B0>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestGetServices()
        {
            ServiceList list = new();
            list.Add(new A0()).Is<BaseClass>();
            list.Add<A1>().Is<BaseClass>();
            new Container(list).Get<BaseClass>(out var services);
            Assert.AreEqual(services.Length, 2);
        }

        [TestMethod]
        public void TestFailedGetServices()
        {
            ServiceList list = new();
            var c = new Container(list);
            try
            {
                c.Get<BaseClass>(out var services);
                Assert.Fail();
            } 
            catch (ContainerException) { }
        }

    }
}