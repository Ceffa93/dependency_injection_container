using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace unit_test
{
    [TestClass]
    public class Tests
    {
        class A0 { public A0() { } };
        class A1 { public A1() { } };
        class A2 { public A2() { } };
        class B0 { public B0(A0 x) { } };
        class B1 { public B1(A1 x) { } };
        class B2 { public B2(A0 x, A1 y) { } };
        class C0 { public C0(A0 x, B0 y) { } };
        class C1 { public C1(B2 x) { } };

        [TestMethod]
        public void TestMultipleRoots()
        {
            ServiceList list = new();
            list.Add<A0>();
            list.Add<A1>();
            Container container = new(list);
        }

        [TestMethod]
        public void TestDiamondInclusion()
        {
            ServiceList list = new();
            list.Add<C0>();
            list.Add<B0>();
            list.Add<A0>();
            Container container = new(list);
        }

        [TestMethod]
        public void TestExternalDependency()
        {
            ServiceList list = new();
            list.Add(new A0());
            list.Add<B0>();
            Container container = new(list);
        }

        [TestMethod]
        public void TestNullExternalDependency()
        {
            ServiceList list = new();
            A0? a0 = null;
            try
            {
                list.Add(a0);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestMissingDependency()
        {
            ServiceList list = new();
            list.Add<C0>();
            try 
            {
                Container container = new(list);
                Assert.Fail();
            }
            catch (ContainerException) {}
        }

        [TestMethod]
        public void TestGetService()
        {
            ServiceList list = new();
            list.Add<A0>();
            list.Add(new A1());

            Container container = new(list);
            container.Get<A0>();
            container.Get<A1>();
        }

        [TestMethod]
        public void TestFailedGet()
        {
            ServiceList list = new();
            Container container = new(list);
            try
            {
                container.Get<A0>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDisposeException()
        {
            ServiceList list = new();
            Container container = new(list);
            container.Dispose();
            try
            {
                container.Get<A0>();
                Assert.Fail();
            }
            catch (ObjectDisposedException) { }
        }


        class TwoConstructor
        {
            public TwoConstructor() { }
            public TwoConstructor(A0 a) { }
        };

        [TestMethod]
        public void TestTwoConstructor()
        {
            ServiceList list = new();
            try
            {
                list.Add<TwoConstructor>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDoubleAdd()
        {
            ServiceList list = new();
            list.Add<A0>();
            try
            {
                list.Add(new A0());
                Assert.Fail();
            }
            catch (ContainerException) { }
        }
    }
}