﻿namespace Flee.OtherTests
{
    using Flee.PublicTypes;
    using Flee.Test.Infrastructure;
    using Flee.OtherTests.ExtensionMethodTestData;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>The extension method test.</summary>
    [TestClass]
    public class ExtensionMethodTest : ExpressionTests
    {
        [TestMethod]
        public void TestExtensionMethodAsMethodCallOnOwner()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(Sub)").Evaluate();
            Assert.AreEqual("Hello as well, SubWorld", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwner()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello()").Evaluate();
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnProperty()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello()").Evaluate();
            Assert.AreEqual("Hello as well, SubWorld", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArguments()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello World!!!", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArgumentsOnOverload()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(true)").Evaluate();
            Assert.AreEqual("Hello dear World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnOwnerWithArgumentsOnClassOverload()
        {
            var result = GetExpressionContext().CompileDynamic("SayHello(2)").Evaluate();
            Assert.AreEqual("hello hello World", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArguments()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello as well, SubWorld!!!", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArgumentsOnClassOverload()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(2)").Evaluate();
            Assert.AreEqual("hello hello SubWorld", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithArgumentsOnOverload()
        {
            var result = GetExpressionContext().CompileDynamic("Sub.SayHello(\"!!!\")").Evaluate();
            Assert.AreEqual("Hello as well, SubWorld!!!", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithNullableArgumentsOnOverload() {
            var result = GetExpressionContext().CompileDynamic("DoubleValue.ConvertDouble(\"abc\")").Evaluate();
            Assert.AreEqual("2", result);
        }

        [TestMethod]
        public void TestExtensionMethodCallOnPropertyWithNullableArgumentsOnOverload2() {
            var result2 = GetExpressionContext().CompileDynamic("NullableDoubleValue.ConvertDouble(\"abc\")").Evaluate();
            Assert.AreEqual("3", result2);
        }

        private static ExpressionContext GetExpressionContext()
        {
            var expressionOwner = new TestData { Id = "World", DoubleValue = 2.0, NullableDoubleValue = 3.0 };
            var context = new ExpressionContext(expressionOwner);
            context.Imports.AddType(typeof(TestDataExtensions));
            return context;
        }
    }
}
