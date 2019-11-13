using System;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flee.Test.CalcEngineTests
{
    [TestClass]
    public class LongScriptTests
    {
        private SimpleCalcEngine _myEngine;
        public LongScriptTests()
        {
            var engine = new SimpleCalcEngine();
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
			//            context.Imports.AddType(typeof(Math), "math");

			//' add convert methods e.g. .ToInt64, .ToString, .ToDateTime...  https://msdn.microsoft.com/en-us/library/system.convert.aspx?f=255&MSPPError=-2147217396
			context.Imports.AddType(typeof(Convert));

			engine.Context = context;
            _myEngine = engine;
        }

        [TestMethod]
        public void LongScriptWithManyFunctions()
        {
			var script = System.IO.File.ReadAllText(@"..\..\..\TestScripts\LongScriptWithManyFunctions.js");
            script = script.Replace('\r', ' ').Replace('\n', ' ');
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(84.0d, result);
		}


		[TestMethod]
		public void FailingLongScriptWithManyFunctions()
		{
			var script = System.IO.File.ReadAllText(@"..\..\..\TestScripts\FailingLongScriptWithManyFunctions.js");
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(84.0d, result);
		}
	}
}
