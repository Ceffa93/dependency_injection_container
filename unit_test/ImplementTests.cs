using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class ImplementTests
    {
        interface IInterface0 { };
        interface IInterface1 { };
        class BaseClass { };

        class A0 : BaseClass, IInterface0, IInterface1 { }
        class A1 : IInterface0 { }
        class B0 { public B0(IInterface0 x) { } }
        class B1 { public B1(IInterface1 x) { } }
        class B2 { public B2(BaseClass x) { } }
        class C { public C(A0 x, BaseClass y, IInterface0 z) { } }


        [TestMethod]
        public void TestInterface()
        {
            ServiceList list = new();
            list.Add<A0>().Is<IInterface0>();
            list.Add<B0>();
            new Container(list);
        }

        [TestMethod]
        public void TestBaseClass()
        {
            ServiceList list = new();
            list.Add<A0>().Is<BaseClass>();
            list.Add<B2>();
            new Container(list);
        }

        [TestMethod]
        public void TestExternalImplement()
        {
            ServiceList list = new();
            A0 a = new A0();
            list.Add(a).Is<BaseClass>();
            list.Add<B2>();
            new Container(list);
        }

        [TestMethod]
        public void TestMultipleImplement()
        {
            ServiceList list = new();
            list.Add<A0>().Is<BaseClass>().Is<IInterface0>().Is<IInterface1>();
            list.Add<B0>();
            list.Add<B1>();
            list.Add<B2>();
            new Container(list);
        }

        [TestMethod]
        public void TestMissingImplement()
        {
            ServiceList list = new();
            list.Add<A0>();
            list.Add<B0>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestWrongImplement()
        {
            ServiceList list = new();
            try
            {
                list.Add<B0>().Is<BaseClass>();
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDuplicatedImplement()
        {
            ServiceList list = new();
            list.Add<A0>().Is<IInterface0>();
            list.Add<A1>().Is<IInterface0>();
            list.Add<B0>();

            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestSelfImplement()
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
        public void TestRepeatedArgument()
        {
            ServiceList list = new();
            list.Add<A0>().Is<BaseClass>().Is<IInterface0>();
            list.Add<C>();
            new Container(list);
        }
    }
}