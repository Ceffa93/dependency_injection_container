using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIC;

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