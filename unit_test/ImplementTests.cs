using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace unit_test
{
    [TestClass]
    public class ImplementTests
    {
        public class BaseClass { };
        public interface IInterface { };
        public class A : BaseClass, IInterface { }
        public class B { public B(IInterface x) { } }

        [TestMethod]
        public void TestImplement()
        {
            ServiceList list = new();
            list.Add<A>().Implement<IInterface>();
            list.Add<B>();
            new Container(list);
        }
    }
}