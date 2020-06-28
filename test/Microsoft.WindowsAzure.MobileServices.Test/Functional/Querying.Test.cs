// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    [TestCategory("live-tests")]
    [TestClass, Ignore]
    public class Querying_Test : FunctionalTestBase
    {
        private async Task Query<T, U>(Func<IMobileServiceTable<T>, IMobileServiceTableQuery<U>> getQuery)
        {
            IMobileServiceTable<T> table = GetClient().GetTable<T>();
            IMobileServiceTableQuery<U> query = getQuery(table);
            await table.ReadAsync(query);
        }

        [TestMethod]
        public async Task LiveBasicQuery()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                from p in table
                select p);
        }

        [TestMethod]
        public async Task LiveOrdering_QuerySyntax()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price ascending
                select p);
        }

        [TestMethod]
        public async Task LiveOrdering_2()
        {
            // Chaining
            await Query<Book, Book>(table => table.OrderBy(p => p.Price));
        }

        [TestMethod]
        public async Task LiveOrdering_QuerySyntaxDescending()
        {
            // Query syntax descending
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price descending
                select p);
        }

        [TestMethod]
        public async Task LiveOrdering_ChainingDescending()
        {
            // Chaining descending
            await Query<Book, Book>(table => table.OrderByDescending(p => p.Price));
        }

        [TestMethod]
        public async Task LiveOrdering_QuerySyntaxMultiple()
        {
            // Query syntax with multiple
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price ascending
                orderby p.Title descending
                select p);
        }

        [TestMethod]
        public async Task LiveOrdering_ChainingWithMultiple()
        {
            // Chaining with multiple
            await Query<Book, Book>(table =>
                table
                .OrderBy(p => p.Price)
                .OrderByDescending(p => p.Title));
        }

        [TestMethod]
        public async Task LiveProjection_QuerySyntax()
        {
            // Query syntax
            await Query<Book, string>(table =>
                from p in table
                select p.Title);
        }

        [TestMethod]
        public async Task LiveProjection_Chaining()
        {
            // Chaining
            await Query<Book, string>(table => table.Select(p => p.Title));
        }

        [TestMethod]
        public async Task LiveProjection_Chaining_2()
        {
            // Chaining
            await Query<Book, Book>(table => table.Select(p => new { x = p.Title })
                                                  .Select(p => new Book() { Title = p.x }));
        }

        [TestMethod]
        public async Task LiveProjection_Negative_1()
        {
            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            await Query((IMobileServiceTable<Book> table) =>
                from p in table
                select new { Foo = p.Title });
        }

        [TestMethod]
        public async Task LiveSkipTake_QuerySyntax()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                (from p in table
                 select p).Skip(2).Take(5));
        }

        [TestMethod]
        public async Task LiveSkipTake_Chaining()
        {
            // Chaining
            await Query<Book, Book>(table => table.Select(p => p).Skip(2).Take(5));
        }

        [TestMethod]
        public async Task LiveFiltering_1()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Price > 50
                select p);
        }

        [TestMethod]
        public async Task LiveFiltering_2()
        {
            await Query<Book, Book>(table => table.Where(p => p.Price > 50));
        }

        [TestMethod]
        public async Task LiveFiltering_3()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10
                select p);
        }

        [TestMethod]
        public async Task LiveFiltering_4()
        {
            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10));
        }

        [TestMethod]
        public async Task LiveFiltering_5()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10 && p.Id > 0
                select p);
        }

        [TestMethod]
        public async Task LiveFiltering_6()
        {
            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10 && p.Id > 0));
        }

        [TestMethod]
        public async Task LiveFiltering_7()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10 || p.Id > 0
                select p);
        }

        [TestMethod]
        public async Task LiveFiltering_8()
        {
            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10 || p.Id > 0));
        }

        [TestMethod]
        public async Task LiveFiltering_9()
        {
            await Query<Book, Book>(table =>
                from p in table
                where !(p.Id > 0)
                select p);
        }

        [TestMethod]
        public async Task LiveFiltering_10()
        {
            await Query<Book, Book>(table => table.Where(p => !(p.Id > 0)));
        }

        [TestMethod]
        public async Task LiveFiltering_11()
        {
            await Query<Book, Book>(table => table.Where(p => (p.Title == "How do I dial this # & such 'things'?")));
        }

        [TestMethod]
        public async Task LiveCombinedQuery()
        {
            await Query((IMobileServiceTable<Book> table) =>
                (from p in table
                 where p.Price <= 10 && p.Advance > 10f
                 where !(p.Id > 0)
                 orderby p.Price descending
                 orderby p.Title
                 select new { p.Title, p.Price })
                .Skip(20)
                .Take(10));
        }

        [TestMethod]
        public async Task LiveFilterOperators_1()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title + "x" == "mx"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_2()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance + 1.0 == 10.0
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_3()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance - 1.0 == 10.0
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_4()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance * 2.0 == 10.0
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_5()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Advance / 2.0 == 10.0
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_6()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Id % 2 == 1
                select p);
        }

        [TestMethod]
        public async Task LiveFilterOperators_7()
        {
            await Query<Book, Book>(table =>
                from p in table
                where (p.Advance * 2.0) / 3.0 + 1.0 == 10.0
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_1()
        {
            // Methods that look like properties
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Length == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_2()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Day == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_3()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Month == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_4()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Year == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_5()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Hour == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_6()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Minute == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_7()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Second == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_8()
        {
            // Static methods
            await Query<Book, Book>(table =>
                from p in table
                where Math.Floor(p.Advance) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_9()
        {
            await Query<Book, Book>(table =>
                from p in table
                where Math.Floor(p.Price) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_10()
        {
            await Query<Book, Book>(table =>
                from p in table
                where Math.Ceiling(p.Advance) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_11()
        {
            await Query<Book, Book>(table =>
                from p in table
                where Math.Ceiling(p.Price) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_12()
        {
            await Query<Book, Book>(table =>
                from p in table
                where Math.Round(p.Advance) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_13()
        {
            await Query<Book, Book>(table =>
                from p in table
                where Math.Round(p.Price) == 10
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_14()
        {
            await Query<Book, Book>(table =>
                from p in table
                where string.Concat(p.Title, "x") == "mx"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_15()
        {
            // Instance methods
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.ToLower() == "a"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_16()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.ToUpper() == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_17()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Trim() == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_18()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.StartsWith("x")
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_19()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.EndsWith("x")
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_20()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Contains("x")
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_21()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.IndexOf("x") == 2
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_22()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.IndexOf('x') == 2
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_23()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Contains("x")
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_24()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Replace("a", "A") == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_25()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Replace('a', 'A') == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_26()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Substring(2) == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_27()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Substring(2, 3) == "A"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_28()
        {
            // Verify each type works on nested expressions too
            await Query<Book, Book>(table =>
                from p in table
                where (p.Title + "x").Length == 7
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_29()
        {
            await Query<Book, Book>(table =>
                from p in table
                where string.Concat(p.Title + "x", "x") == "mx"
                select p);
        }

        [TestMethod]
        public async Task LiveFilterMethods_30()
        {
            await Query<Book, Book>(table =>
                from p in table
                where (p.Title + "x").ToLower() == "ax"
                select p);
        }
    }
}
