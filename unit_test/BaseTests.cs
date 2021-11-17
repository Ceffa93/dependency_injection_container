using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DI;

namespace unit_test
{
    [TestClass]
    public class BaseTests
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
            new Container(list);
        }

        [TestMethod]
        public void TestDiamondInclusion()
        {
            ServiceList list = new();
            list.Add<C0>();
            list.Add<B0>();
            list.Add<A0>();
            new Container(list);
        }

        [TestMethod]
        public void TestExternalDependency()
        {
            ServiceList list = new();
            list.Add(new A0());
            list.Add<B0>();
            new Container(list);
        }

        [TestMethod]
        public void TestNullExternalDependency()
        {
            ServiceList list = new();
            A0? a0 = null;
            try
            {
#pragma warning disable CS8634 
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
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) {}
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
            list.Add<TwoConstructor>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDoubleInternal()
        {
            ServiceList list = new();
            list.Add<A0>();
            list.Add<A0>();
        }

        [TestMethod]
        public void TestInternalExternal()
        {
            ServiceList list = new();
            list.Add<A0>();
            list.Add(new A0());
            list.Add<A0>();
        }

        [TestMethod]
        public void TestDoubleExternalDifferent()
        {
            ServiceList list = new();
            list.Add(new A0());
            try
            {
                list.Add(new A0());
                Assert.Fail();
            } 
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDoubleExternalSame()
        {
            ServiceList list = new();
            A0 a = new();
            list.Add(a);
            list.Add(a);
        }
    }
}