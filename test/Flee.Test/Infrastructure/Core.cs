using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using System.Threading;
using System.Xml.XPath;
using Flee.PublicTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flee.Test.Infrastructure
{

    public abstract class ExpressionTests
{
    private const string COMMENT_CHAR = "\'";
    private const char SEPARATOR_CHAR = ';';

    protected delegate void LineProcessor(string[] lineParts);

    protected ExpressionOwner MyValidExpressionsOwner = new ExpressionOwner();
    protected ExpressionContext MyGenericContext;
    protected ExpressionContext MyValidCastsContext;
    protected ExpressionContext MyCurrentContext;

    protected static readonly CultureInfo TestCulture = CultureInfo.GetCultureInfo("en-CA");

    public ExpressionTests()
    {

        // Set the correct culture, otherwise tests will fail
        Thread.CurrentThread.CurrentCulture = TestCulture;

        MyValidExpressionsOwner = new ExpressionOwner();

        MyGenericContext = this.CreateGenericContext(MyValidExpressionsOwner);

        var context = new ExpressionContext(MyValidExpressionsOwner);
        context.Options.OwnerMemberAccess = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        context.Imports.ImportBuiltinTypes();
        context.Imports.AddType(typeof(Convert), "Convert");
        context.Imports.AddType(typeof(Guid));
        context.Imports.AddType(typeof(Version));
        context.Imports.AddType(typeof(DayOfWeek));
        context.Imports.AddType(typeof(DayOfWeek), "DayOfWeek");
        context.Imports.AddType(typeof(ValueType));
        context.Imports.AddType(typeof(IComparable));
        context.Imports.AddType(typeof(ICloneable));
        context.Imports.AddType(typeof(Array));
        context.Imports.AddType(typeof(System.Delegate));
        // context.Imports.AddType(typeof(AppDomainInitializer));
        context.Imports.AddType(typeof(System.Text.Encoding));
        context.Imports.AddType(typeof(System.Text.ASCIIEncoding));
        context.Imports.AddType(typeof(ArgumentException));

        MyValidCastsContext = context;

        // For testing virtual properties
        TypeDescriptor.AddProvider(new UselessTypeDescriptionProvider(TypeDescriptor.GetProvider(typeof(int))), typeof(int));
        TypeDescriptor.AddProvider(new UselessTypeDescriptionProvider(TypeDescriptor.GetProvider(typeof(string))), typeof(string));

        this.Initialize();
    }

    protected virtual void Initialize()
    {
    }

    protected ExpressionContext CreateGenericContext(object owner)
    {
        ExpressionContext context;

        if (owner == null)
            context = new ExpressionContext();
        else
            context = new ExpressionContext(owner);

        context.Options.OwnerMemberAccess = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        context.Imports.ImportBuiltinTypes();
        context.Imports.AddType(typeof(Math), "Math");
        context.Imports.AddType(typeof(Uri), "Uri");
        // context.Imports.AddType(typeof(Mouse), "Mouse");
        context.Imports.AddType(typeof(Monitor), "Monitor");
        context.Imports.AddType(typeof(DateTime), "DateTime");
        context.Imports.AddType(typeof(Convert), "Convert");
        context.Imports.AddType(typeof(Type), "Type");
        context.Imports.AddType(typeof(DayOfWeek), "DayOfWeek");
        context.Imports.AddType(typeof(ConsoleModifiers), "ConsoleModifiers");

        var ns1 = new NamespaceImport("ns1");
        var ns2 = new NamespaceImport("ns2");
        ns2.Add(new TypeImport(typeof(Math)));

        ns1.Add(ns2);

        context.Imports.RootImport.Add(ns1);

        context.Variables.Add("varInt32", 100);
        context.Variables.Add("varDecimal", new decimal(100));
        context.Variables.Add("varString", "string");

        return context;
    }


    protected IGenericExpression<T> CreateGenericExpression<T>(string expression)
    {
        return this.CreateGenericExpression<T>(expression, new ExpressionContext());
    }

    protected IGenericExpression<T> CreateGenericExpression<T>(string expression, ExpressionContext context)
    {
        var e = context.CompileGeneric<T>(expression);
        return e;
    }

    protected IDynamicExpression CreateDynamicExpression(string expression)
    {
        return this.CreateDynamicExpression(expression, new ExpressionContext());
    }

    protected IDynamicExpression CreateDynamicExpression(string expression, ExpressionContext context)
    {
        return context.CompileDynamic(expression);
    }

    protected void AssertCompileException(string expression)
    {
        try
        {
            this.CreateDynamicExpression(expression);
            Assert.Fail();
        }
        catch (ExpressionCompileException)
        {
        }
    }

    protected void AssertCompileException(string expression, ExpressionContext context, CompileExceptionReason? expectedReason = null)
    {
        try
        {
            this.CreateDynamicExpression(expression, context);
            Assert.Fail($"Compile exception expected for {expression}");
        }
        catch (ExpressionCompileException ex)
        {
            if (expectedReason != null)
                Assert.AreEqual(expectedReason, ex.Reason, $"Expected reason '{expectedReason}' but got '{ex.Reason}'");
        }
    }

    protected void DoTest(IDynamicExpression e, string result, Type resultType, CultureInfo testCulture)
    {
        if (resultType == typeof(object))
        {
            var expectedType = Type.GetType(result, false, true);

            if (expectedType == null)
            {
                // Try to get the type from the Tests assembly
                result = $"{this.GetType().Namespace}.{result}";
                expectedType = this.GetType().Assembly.GetType(result, true, true);
            }

            var expressionResult = e.Evaluate();

            if (expectedType == typeof(void))
                Assert.IsNull(expressionResult, $"{e.Text} should be null, but is {expressionResult}");
            else
                Assert.IsInstanceOfType(expressionResult, expectedType, $"{e.Text} should be of type {expectedType}, but is {expressionResult.GetType()}");
        }
        else
        {
            var tc = TypeDescriptor.GetConverter(resultType);

            var expectedResult = tc.ConvertFromString(null, testCulture, result);
            var actualResult = e.Evaluate();

            expectedResult = RoundIfReal(expectedResult);
            actualResult = RoundIfReal(actualResult);

            Assert.AreEqual(expectedResult, actualResult, $"{e.Text} should be {expectedResult} but is {actualResult}");
        }
    }

    protected object RoundIfReal(object value)
    {
        if (value.GetType() == typeof(double))
        {
            var d = (double)value;
            d = Math.Round(d, 4);
            return d;
        }
        else if (value.GetType() == typeof(float))
        {
            var s = (float)value;
            s = (float) Math.Round(s, 4);
            return s;
        }
        else
            return value;
    }

    protected void ProcessScriptTests(string scriptFileName, LineProcessor processor)
    {
        this.WriteMessage("Testing: {0}", scriptFileName);

        var instream = this.GetScriptFile(scriptFileName);
        var sr = new System.IO.StreamReader(instream);

        try
        {
            this.ProcessLines(sr, processor);
        }
        finally
        {
            sr.Close();
        }
    }

    protected void ProcessLines(System.IO.TextReader sr, LineProcessor processor)
    {
        while (sr.Peek() != -1)
        {
            var line = sr.ReadLine();
            this.ProcessLine(line, processor);
        }
    }

    protected void ProcessLine(string line, LineProcessor processor)
    {
        if (line.StartsWith(COMMENT_CHAR))
            return;

        try
        {
            var arr = line.Split(SEPARATOR_CHAR);
            processor(arr);
        }
        catch (Exception)
        {
            this.WriteMessage("Failed line: {0}", line);
            throw;
        }
    }

    protected System.IO.Stream GetScriptFile(string fileName)
    {
        var a = Assembly.GetExecutingAssembly();
        return a.GetManifestResourceStream(/*this.GetType(),*/ a.GetName().Name + ".TestScripts." + fileName);
    }

    protected string GetIndividualTest(string testName)
    {
        var a = Assembly.GetExecutingAssembly();
        using (var s = a.GetManifestResourceStream(this.GetType(), "IndividualTests.xml"))
        {
            var doc = new XPathDocument(s);
            var nav = doc.CreateNavigator();
            nav = nav.SelectSingleNode($"Tests/Test[@Name='{testName}']");

            var str = (string)nav.TypedValue;
            s.Close();

            return str;
        }
    }

    protected void WriteMessage(string msg, params object[] args)
    {
        msg = string.Format(msg, args);
        Console.WriteLine(msg);
    }



    protected static object Parse(string s)
    {
        bool b;

        if (bool.TryParse(s, out b))
            return b;

        if (int.TryParse(s, NumberStyles.Integer, TestCulture, out var i))
            return i;

        if (double.TryParse(s, NumberStyles.Float, TestCulture, out var d))
            return d;

        if (DateTime.TryParse(s, TestCulture, DateTimeStyles.None, out var dt))
            return dt;

        return s;
    }

    protected static IDictionary<string, object> ParseQueryString(string s)
    {
        var arr = s.Split('&');
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in arr)
        {
            var arr2 = part.Split('=');
            dict.Add(arr2[0], Parse(arr2[1]));
        }

        return dict;
    }
}
}

