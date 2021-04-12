using System.Collections.Generic;
using Flee.PublicTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FleeExt.Test.ExpressionTests
{
    [TestClass]
    public class InterfaceTest
    {
        [TestMethod]
        public void TestReadonlyListCount()
        {
            var result = GetExpressionContext().CompileDynamic($"{nameof(InterfaceTestClass.ReadOnlyList)}.Count").Evaluate();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestBaseInterfaceProperty()
        {
            var result = GetExpressionContext().CompileDynamic($"{nameof(InterfaceTestClass.InterfaceTest)}.{nameof(ISubInterface.BaseProperty)}").Evaluate();
            Assert.AreEqual("base", result);
        }

        [TestMethod]
        public void TestSubInterfaceProperty()
        {
            var result = GetExpressionContext().CompileDynamic($"{nameof(InterfaceTestClass.InterfaceTest)}.{nameof(ISubInterface.SubProperty)}").Evaluate();
            Assert.AreEqual("sub", result);
        }

        [TestMethod]
        public void TestBaseInterfaceMethod()
        {
            var result = GetExpressionContext().CompileDynamic($"{nameof(InterfaceTestClass.InterfaceTest)}.{nameof(ISubInterface.BaseMethod)}()").Evaluate();
            Assert.AreEqual("base", result);
        }

        [TestMethod]
        public void TestSubInterfaceMethod()
        {
            var result = GetExpressionContext().CompileDynamic($"{nameof(InterfaceTestClass.InterfaceTest)}.{nameof(ISubInterface.SubMethod)}()").Evaluate();
            Assert.AreEqual("sub", result);
        }

        private static ExpressionContext GetExpressionContext()
        {
            var expressionOwner = new InterfaceTestClass
            {
                ReadOnlyList = new List<ISubInterface>
                {
                    new SubImplementation() {BaseProperty = "baseInList", SubProperty = "subInList"}
                },
                InterfaceTest = new SubImplementation() { BaseProperty = "base", SubProperty = "sub" }
            };

            var context = new ExpressionContext(expressionOwner);
            context.Imports.AddType(typeof(InterfaceTestClass));
            return context;
        }

    }

    public interface IBaseInterface
    {
        string BaseProperty { get; set; }

        string BaseMethod();
    }

    public interface ISubInterface : IBaseInterface
    {
        string SubProperty { get; set; }

        string SubMethod();
    }

    public class SubImplementation : ISubInterface
    {
        public string BaseProperty { get; set; }
        public string BaseMethod() => BaseProperty;

        public string SubProperty { get; set; }
        public string SubMethod() => SubProperty;
    }

    public class InterfaceTestClass
    {
        public ISubInterface InterfaceTest { get; set; }

        public IReadOnlyList<ISubInterface> ReadOnlyList { get; set; }
    }
}
