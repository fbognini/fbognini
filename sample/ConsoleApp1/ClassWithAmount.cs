using fbognini.Core.Utilities;

namespace ConsoleApp1
{
    public class ClassWithAmount
    {
        public string Name { get; set; }
        [Amount]
        public double FinalAmount { get; set; }
        [Amount]
        public double TotalAmount { get; set; }

        [Amount]
        public List<double> Amounts { get; set; }

        public NestedClassWithAmount Nested { get; set; }
        public IEnumerable<NestedClassWithAmount> Nesteds { get; set; }
    }

    public class NestedClassWithAmount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Amount]
        public double Amount { get; set; }

        public NestedClassWithAmount(double amount)
        {
            Amount = amount;
        }
    }
}
