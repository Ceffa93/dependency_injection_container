using Microsoft.VisualStudio.TestTools.UnitTesting;
using DI;

namespace unit_test
{
    [TestClass]
    public class CircularTests
    {
        class CycleA { public CycleA(CycleB x) { } }
        class CycleB { public CycleB(CycleA x) { } }
        class CycleUser { public CycleUser(CycleA x) { } }


        [TestMethod]
        public void TestConnectedCircular()
        {
            ServiceList list = new();
            list.Add<CycleA>();
            list.Add<CycleB>();
            list.Add<CycleUser>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDisconnectedCircular()
        {
            ServiceList list = new();
            list.Add<CycleA>();
            list.Add<CycleB>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        class SelfDep { public SelfDep(SelfDep x) { } }
        class SelfDepUser { public SelfDepUser(SelfDep x) { } }


        [TestMethod]
        public void TestConnectedSelfDependency()
        {
            ServiceList list = new();
            list.Add<SelfDep>();
            list.Add<SelfDepUser>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }

        [TestMethod]
        public void TestDisconnectedSelfDependency()
        {
            ServiceList list = new();
            list.Add<SelfDep>();
            try
            {
                new Container(list);
                Assert.Fail();
            }
            catch (ContainerException) { }
        }
    }
}