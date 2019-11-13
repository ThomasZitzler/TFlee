using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flee.PublicTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flee.Test.ExpressionTests
{
    [TestClass]
    public class Benchmarks : Core
    {
        [TestMethod /*(Description = "Test that setting variables is fast")*/]
        public void TestFastVariables()
        {
            //Test should take 200ms or less
            const int expectedTime = 200;
            const int iterations = 100000;

            var context = new ExpressionContext();
            var vars = context.Variables;
            vars.DefineVariable("a", typeof(int));
            vars.DefineVariable("b", typeof(int));
            IDynamicExpression e = this.CreateDynamicExpression("a + b", context);

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations - 1; i++)
            {
                vars["a"] = 200;
                vars["b"] = 300;
                var result = e.Evaluate();
            }
            sw.Stop();
            this.PrintSpeedMessage("Fast variables", iterations, sw);
            Assert.IsTrue(sw.ElapsedMilliseconds < expectedTime, $"Test time {sw.ElapsedMilliseconds} above expected value {expectedTime}");
        }

        private void PrintSpeedMessage(string title, int iterations, Stopwatch sw)
        {
            this.WriteMessage("{0}: {1:n0} iterations in {2:n2}ms = {3:n2} iterations/sec", title, iterations, sw.ElapsedMilliseconds, iterations / (sw.ElapsedMilliseconds / 10));
        }
    }
}
