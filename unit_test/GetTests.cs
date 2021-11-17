using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DI;

namespace unit_test
{
    [TestClass]
    public class GetTests
    {
        class A { public A() { } };

        [TestMethod]
        public void TestGetInternal()
        {
            ServiceList list = new();
            list.Add<A>();
            new Container(list).Get<A>();
        }

        [TestMethod]
        public void TestGetExternal()
        {
            ServiceList list = new();
            list.Add(new A());
            new Container(list).Get<A>();
        }

        [TestMethod]
        public void TestFailedGet()
        {
            ServiceList list = new();
            Container container = new(list);
            try
            {
                container.Get<A>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        class Base { }
        class B0 : Base { public B0() { } };
        class B1 : Base { public B1() { } };


        [TestMethod]
        public void TestGetServices()
        {
            ServiceList list = new();
            list.Add(new B0()).Is<Base>();
            list.Add<B1>().Is<Base>();
            new Container(list).Get<Base>(out var services);
            Assert.AreEqual(services.Length, 2);
        }

        [TestMethod]
        public void TestFailedGetServices()
        {
            ServiceList list = new();
            var c = new Container(list);
            try
            {
                c.Get<Base>(out var services);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }
    }
}