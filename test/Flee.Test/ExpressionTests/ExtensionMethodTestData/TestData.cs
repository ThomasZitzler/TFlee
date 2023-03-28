namespace Flee.OtherTests.ExtensionMethodTestData
{
    internal class TestData
    {
        public string Id { get; set; }

        public double DoubleValue { get; set; }

        public double? NullableDoubleValue { get; set; }

        public SubTestData Sub => new SubTestData { Id = "Sub" + this.Id };

        public string VarCode { get; internal set; }

        public string SayHello(int times)
        {
            string result = string.Empty;
            for (int i = 0; i < times; i++)
            {
                result += "hello ";
            }

            return result + Id;
        }
    }

    internal class SubTestData
    {
        public string Id { get; set; }

        public string SayHello(int times)
        {
            string result = string.Empty;
            for (int i = 0; i < times; i++)
            {
                result += "hello ";
            }

            return result + Id;
        }
    }
}
