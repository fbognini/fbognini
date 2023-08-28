using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Tests.Integration.Fixture.Entities.Seeds
{
    internal class AuthorSeed
    {
        public const int Total = 10;
        public const string FirstLastName = "AAAAA";
        public const string LastLastName = "ZZZZZ";

        public static readonly Faker<Author> Faker = new Faker<Author>()
            .RuleFor(x => x.FirstName, faker => faker.Person.FirstName)
            .RuleFor(x => x.LastName, faker => faker.Person.LastName);

        public static List<Author> GetAuthors()
        {
            var authors = new List<Author>();
            for (int i = 0; i < Total; i++)
            {
                var author = Faker.Generate();
                authors.Add(author);
            }

            authors[Total - 1].LastName = LastLastName;
            authors[Total - 2].LastName = FirstLastName;

            return authors;
        }
    }
}
