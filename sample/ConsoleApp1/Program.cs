// See https://aka.ms/new-console-template for more information
using ConsoleApp1;
using fbognini.Core.Exceptions;
using fbognini.Core.Utilities;
using System.Net.Http.Json;

Console.WriteLine("Hello, World!");

var criteria = new FooCriteria()
{
    Title = "ciccio"
};
criteria.Search.Keyword = "foo";
criteria.Search.Fields.Add(x => x.FirstName);
criteria.Search.FieldStrings.Add("Ciccio");
criteria.AddSorting("Criteria", fbognini.Core.Domain.Query.SortingDirection.ASCENDING);
criteria.AddSorting("Criteria DESSC", fbognini.Core.Domain.Query.SortingDirection.DESCENDING);


using (var httpClient = new HttpClient())
{
    for (int i = 0; i < 100; i++)
    {
        var request = new { firstName = "ciccio", lastName = "pasticcio" };

        await httpClient.PostAsJsonAsync("https://localhost:7286/authors", request);
    }
}


throw new NotFoundException<Foo>(criteria);

var classWithAmount = new ClassWithAmount()
{
    Name = "Test",
    TotalAmount = 100,
    FinalAmount = 90,
    Amounts = new List<double> { 10, 20, 30 },
    Nested = new NestedClassWithAmount(20),
    Nesteds = new List<NestedClassWithAmount>()
    {
        new NestedClassWithAmount(50),
        new NestedClassWithAmount(60)
    }
};

var conversionRate = new AmountConversionRate()
{
    Ratio = 1,
    ExtraMarginPercentage = 5,
    ExtraMarginValue = 0
};

AmountExtraMargin.ApplyConversion(classWithAmount, conversionRate);