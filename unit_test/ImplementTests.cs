using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIC;

namespace unit_test
{
    [TestClass]
    public class ImplementTests
    {
        public interface IInterface0 { };
        public interface IInterface1 { };
        public class BaseClass { };

        public class A : BaseClass, IInterface0, IInterface1 { }
        public class B0 { public B0(IInterface0 x) { } }
        public class B1 { public B1(IInterface1 x) { } }
        public class B2 { public B2(BaseClass x) { } }


        [TestMethod]
        public void TestInterface()
        {
            ServiceList list = new();
            list.Add<A>().Is<IInterface0>();
            list.Add<B0>();
            new Container(list);
        }

        [TestMethod]
        public void TestBaseClass()
        {
            ServiceList list = new();
            list.Add<A>().Is<BaseClass>();
            list.Add<B2>();
            new Container(list);
        }

        [TestMethod]
        public void TestMultipleImplement()
        {
            ServiceList list = new();
            list.Add<A>().Is<BaseClass>().Is<IInterface0>().Is<IInterface1>();
            list.Add<B0>();
            list.Add<B1>();
            list.Add<B2>();
            new Container(list);
        }

        [TestMethod]
        public void TestMissingImplement()
        {
            ServiceList list = new();
            list.Add<A>();
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
    }
}