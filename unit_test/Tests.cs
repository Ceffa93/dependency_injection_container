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
            Container container = new();
            container.Add<A0>();
            container.Add<A1>();
            container.Construct();
        }

        [TestMethod]
        public void TestDiamondInclusion()
        {
            Container container = new();
            container.Add<C0>();
            container.Add<B0>();
            container.Add<A0>();
            container.Construct();
        }

        [TestMethod]
        public void TestExternalDependency()
        {
            Container container = new();
            container.Add(new A0());
            container.Add<B0>();
            container.Construct();
        }

        [TestMethod]
        public void TestNullExternalDependency()
        {
            Container container = new();
            A0? a0 = null;
            try
            {
                container.Add(a0);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestMissingDependency()
        {
            Container container = new();
            container.Add<C0>();
            try 
            {
                container.Construct();
                Assert.Fail();
            }
            catch (ContainerException) {}
        }

        [TestMethod]
        public void TestGetService()
        {
            Container container = new();
            container.Add<A0>();
            container.Add(new A1());
            container.Construct();
            container.Get<A0>();
            container.Get<A1>();
        }

        [TestMethod]
        public void TestFailedGet()
        {
            Container container = new();
            container.Construct();
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
            Container container = new();
            container.Dispose();
            try
            {
                container.Construct();
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
            Container container = new();
            try
            {
                container.Add<TwoConstructor>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }
    }
}