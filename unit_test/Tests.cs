using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace unit_test
{
    [TestClass]
    public class Tests
    {
        #region test_classes
        struct A0 { public A0() { } };
        struct A1 { public A1() { } };
        struct A2 { public A2() { } };
        struct B0 { public B0(A0 x) { } };
        struct B1 { public B1(A1 x) { } };
        struct B2 { public B2(A0 x, A1 y) { } };
        struct C0 { public C0(A0 x, B0 y) { } };
        struct C1 { public C1(B2 x) { } };
        #endregion

        [TestMethod]
        public void TestMultipleRoots()
        {
            A0 a = new();
            Container container = new();
            container.Add<A0>();
            container.Add<A1>();
            container.Construct();
            container.Dispose();
        }

        [TestMethod]
        public void TestDiamondInclusion()
        {
            Container container = new();
            container.Add<C0>();
            container.Add<B0>();
            container.Add<A0>();
            container.Construct();
            container.Dispose();
        }

        [TestMethod]
        public void TestMissingDependency()
        {
            Container container = new();
            container.Add<C0>();
            try 
            {
                container.Construct();
                container.Dispose();
                Assert.Fail();
            }
            catch (ContainerException) {}
        }
    }
}