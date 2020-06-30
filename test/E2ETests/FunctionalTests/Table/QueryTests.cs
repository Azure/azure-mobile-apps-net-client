// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("Query")]
    public class QueryTests : FunctionalTestBase
    {
        private const int VeryLargeTopValue = 1001;


        [AsyncTestMethod]
        public async Task SimpleDataSource()
        {
            Log("This test is known to fail in Xamarin -- see https://bugzilla.xamarin.com/show_bug.cgi?id=22955");
            // Get the Movie table
            IMobileServiceTable<Movie> table = GetClient().GetTable<Movie>();

            // Create a new CollectionView
            Log("Creating DataSource");
            MobileServiceCollection<Movie, Movie> dataSource =
                await table.Take(1000).OrderByDescending(m => m.Title).ToCollectionAsync();

            Log("Verifying loaded");
            Assert.AreEqual(248, dataSource.Count);
            Assert.AreEqual((long)-1, ((ITotalCountProvider)dataSource).TotalCount);
            Assert.AreEqual("Yojimbo", dataSource[0].Title);
        }

        [AsyncTestMethod]
        public async Task SimpleDataSourceWithTotalCount()
        {
            // Get the Movie table
            IMobileServiceTable<Movie> table = GetClient().GetTable<Movie>();

            // Create a new CollectionView
            Log("Creating DataSource");
            MobileServiceCollection<Movie, Movie> dataSource =
                await table.Take(5).IncludeTotalCount().ToCollectionAsync();

            Log("Verifying loaded");
            Assert.AreEqual(5, dataSource.Count);
            Assert.AreEqual((long)248, ((ITotalCountProvider)dataSource).TotalCount);
        }

        [AsyncTestMethod]
        private async Task QueryTests_Common()
        {
            Log("This test is known to fail in Xamarin -- see https://bugzilla.xamarin.com/show_bug.cgi?id=22955");
            // Numeric fields
            await CreateQueryTestIntId("GreaterThan and LessThan - Movies from the 90s", m => m.Year > 1989 && m.Year < 2000);
            await CreateQueryTestIntId("GreaterEqual and LessEqual - Movies from the 90s", m => m.Year >= 1990 && m.Year <= 1999);
            await CreateQueryTestIntId("Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960)));
            await CreateQueryTestIntId("Division, equal and different - Movies from the year 2001 with rating other than R",
                m => ((m.Year / 1000.5) == 2) && (m.MpaaRating != "R"));
            await CreateQueryTestIntId("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120));

            await CreateQueryTestStringId("GreaterThan and LessThan - Movies from the 90s", m => m.Year > 1989 && m.Year < 2000);
            await CreateQueryTestStringId("GreaterEqual and LessEqual - Movies from the 90s", m => m.Year >= 1990 && m.Year <= 1999);
            await CreateQueryTestStringId("Compound statement - OR of ANDs - Movies from the 30s and 50s",
                m => ((m.Year >= 1930) && (m.Year < 1940)) || ((m.Year >= 1950) && (m.Year < 1960)));
            await CreateQueryTestStringId("Division, equal and different - Movies from the year 2001 with rating other than R",
                m => ((m.Year / 1000.5) == 2) && (m.MpaaRating != "R"));
            await CreateQueryTestStringId("Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours",
                m => ((m.Year - 1900) >= 80) && (m.Year + 10 < 2000) && (m.Duration < 120));

            // String functions
            await CreateQueryTestIntId("String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100);
            await CreateQueryTestIntId("String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100);
            await CreateQueryTestIntId("String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r"));
            await CreateQueryTestIntId("String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE"));
            await CreateQueryTestIntId("String: Length - Movies with small names",
                m => m.Title.Length < 10, 200);
            await CreateQueryTestIntId("String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r");
            await CreateQueryTestIntId("String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r");
            await CreateQueryTestIntId("String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MpaaRating, "-13").StartsWith("PG-13"));

            await CreateQueryTestStringId("String: StartsWith - Movies which starts with 'The'",
                m => m.Title.StartsWith("The"), 100);
            await CreateQueryTestStringId("String: StartsWith, case insensitive - Movies which start with 'the'",
                m => m.Title.ToLower().StartsWith("the"), 100);
            await CreateQueryTestStringId("String: EndsWith, case insensitive - Movies which end with 'r'",
                m => m.Title.ToLower().EndsWith("r"));
            await CreateQueryTestStringId("String: Contains - Movies which contain the word 'one', case insensitive",
                m => m.Title.ToUpper().Contains("ONE"));
            await CreateQueryTestStringId("String: Length - Movies with small names",
                m => m.Title.Length < 10, 200);
            await CreateQueryTestStringId("String: Substring (1 parameter), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1) == "r");
            await CreateQueryTestStringId("String: Substring (2 parameters), length - Movies which end with 'r'",
                m => m.Title.Substring(m.Title.Length - 1, 1) == "r");
            await CreateQueryTestStringId("String: Concat - Movies rated 'PG' or 'PG-13' from the 2000s",
                m => m.Year >= 2000 && string.Concat(m.MpaaRating, "-13").StartsWith("PG-13"));

            // String fields
            await CreateQueryTestIntId("String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MpaaRating == "PG-13", 100);
            await CreateQueryTestIntId("String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MpaaRating == null,
   whereLambda: m => m.Year >= 1980 && m.MpaaRating == null);
            await CreateQueryTestIntId("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MpaaRating != null,
   whereLambda: m => m.Year < 1970 && m.MpaaRating != null);

            await CreateQueryTestStringId("String equals - Movies since 1980 with rating PG-13",
                m => m.Year >= 1980 && m.MpaaRating == "PG-13", 100);
            await CreateQueryTestStringId("String field, comparison to null - Movies since 1980 without a MPAA rating",
                m => m.Year >= 1980 && m.MpaaRating == null,
   whereLambda: m => m.Year >= 1980 && m.MpaaRating == null);
            await CreateQueryTestStringId("String field, comparison (not equal) to null - Movies before 1970 with a MPAA rating",
                m => m.Year < 1970 && m.MpaaRating != null,
   whereLambda: m => m.Year < 1970 && m.MpaaRating != null);

            // Numeric functions
            await CreateQueryTestIntId("Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3);
            await CreateQueryTestIntId("Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2);
            await CreateQueryTestIntId("Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2);

            await CreateQueryTestStringId("Floor - Movies which last more than 3 hours",
                m => Math.Floor(m.Duration / 60.0) >= 3);
            await CreateQueryTestStringId("Ceiling - Best picture winners which last at most 2 hours",
                m => m.BestPictureWinner == true && Math.Ceiling(m.Duration / 60.0) == 2);
            await CreateQueryTestStringId("Round - Best picture winners which last more than 2.5 hours",
                m => m.BestPictureWinner == true && Math.Round(m.Duration / 60.0) > 2);

            // Date fields
            await CreateQueryTestIntId("Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateQueryTestIntId("Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc));
            await CreateQueryTestIntId("Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc));

            await CreateQueryTestStringId("Date: Greater than, less than - Movies with release date in the 70s",
                m => m.ReleaseDate > new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateQueryTestStringId("Date: Greater than, less than - Movies with release date in the 80s",
                m => m.ReleaseDate >= new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                    m.ReleaseDate < new DateTime(1989, 12, 31, 23, 59, 59, DateTimeKind.Utc));
            await CreateQueryTestStringId("Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)",
                m => m.ReleaseDate == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc));

            // Date functions
            await CreateQueryTestIntId("Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11);
            await CreateQueryTestIntId("Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1);
            await CreateQueryTestIntId("Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100);

            await CreateQueryTestStringId("Date (month): Movies released in November",
                m => m.ReleaseDate.Month == 11);
            await CreateQueryTestStringId("Date (day): Movies released in the first day of the month",
                m => m.ReleaseDate.Day == 1);
            await CreateQueryTestStringId("Date (year): Movies whose year is different than its release year",
                m => m.ReleaseDate.Year != m.Year, 100);

            // Bool fields
            await CreateQueryTestIntId("Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true);
            await CreateQueryTestIntId("Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false));
            await CreateQueryTestIntId("Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000);

            await CreateQueryTestStringId("Bool: equal to true - Best picture winners before 1950",
                m => m.Year < 1950 && m.BestPictureWinner == true);
            await CreateQueryTestStringId("Bool: equal to false - Best picture winners after 2000",
                m => m.Year >= 2000 && !(m.BestPictureWinner == false));
            await CreateQueryTestStringId("Bool: not equal to false - Best picture winners after 2000",
                m => m.BestPictureWinner != false && m.Year >= 2000);

            // Top and skip
            await CreateQueryTestIntId("Get all using large $top - 500", null, 500);
            await CreateQueryTestIntId("Skip all using large skip - 500", null, null, 500, new[] { new OrderByClause("Title", true) });
            await CreateQueryTestIntId("Get first ($top) - 10", null, 10);
            await CreateQueryTestIntId("Get last ($skip) - 10", null, null, QueryTestData.TestIntIdMovies.Length - 10, new[] { new OrderByClause("Title", true) });
            await CreateQueryTestIntId("Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true);
            await CreateQueryTestIntId("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true);
            await CreateQueryTestStringId("Get all using large $top - 500", null, 500);
            await CreateQueryTestStringId("Skip all using large skip - 500", null, null, 500);
            await CreateQueryTestStringId("Get first ($top) - 10", null, 10);
            await CreateQueryTestStringId("Get last ($skip) - 10", null, null, QueryTestData.TestMovies().Length - 10);
            await CreateQueryTestStringId("Skip, take, includeTotalCount - movies 11-20, ordered by title",
                null, 10, 10, new[] { new OrderByClause("Title", true) }, null, true);
            await CreateQueryTestStringId("Skip, take, filter includeTotalCount - movies 11-20 which won a best picture award, ordered by year",
                m => m.BestPictureWinner == true, 10, 10, new[] { new OrderByClause("Year", false) }, null, true);

            // Order by
            await CreateQueryTestIntId("Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) });
            await CreateQueryTestIntId("Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true);

            await CreateQueryTestStringId("Order by date and string - 50 movies, ordered by release date, then title",
                null, 50, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) });
            await CreateQueryTestStringId("Order by number - 30 shortest movies since 1970",
                m => m.Year >= 1970, 30, null, new[] { new OrderByClause("Duration", true), new OrderByClause("Title", true) }, null, true);

            // Select
            await CreateQueryTestIntId("Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title);
            await CreateQueryTestIntId("Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration));

            await CreateQueryTestStringId("Select one field - Only title of movies from 2008",
                m => m.Year == 2008, null, null, null, m => m.Title);
            await CreateQueryTestStringId("Select multiple fields - Nicely formatted list of movies from the 2000's",
                m => m.Year >= 2000, 200, null, new[] { new OrderByClause("ReleaseDate", false), new OrderByClause("Title", true) },
                m => string.Format("{0} {1} - {2} minutes", m.Title.PadRight(30), m.BestPictureWinner ? "(best picture)" : "", m.Duration));

            // Tests passing the OData query directly to the Read operation
            await CreateQueryTestIntId("Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc");

            await CreateQueryTestStringId("Passing OData query directly - movies from the 80's, ordered by Title, items 3, 4 and 5",
                whereClause: m => m.Year >= 1980 && m.Year <= 1989,
                top: 3, skip: 2,
                orderBy: new OrderByClause[] { new OrderByClause("Title", true) },
                odataQueryExpression: "$filter=((Year ge 1980) and (Year le 1989))&$top=3&$skip=2&$orderby=Title asc");

            // Negative tests
            await CreateQueryTest<IntIdMovie, MobileServiceInvalidOperationException>("[Int id] (Neg) Very large top value", m => m.Year > 2000, VeryLargeTopValue);
            await CreateQueryTest<Movie, MobileServiceInvalidOperationException>("[String id] (Neg) Very large top value", m => m.Year > 2000, VeryLargeTopValue);
            await CreateQueryTest<IntIdMovie, NotSupportedException>("[Int id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43);
            await CreateQueryTest<Movie, NotSupportedException>("[String id] (Neg) Unsupported predicate: unsupported arithmetic",
                m => Math.Sqrt(m.Year) > 43);

            // Invalid lookup
            for (int i = -1; i <= 0; i++)
            {
                int id = i;
                //(Neg) Invalid id for lookup: " + i

                var table = this.GetClient().GetTable<IntIdMovie>();
                try
                {
                    var item = await table.LookupAsync(id);
                    Log("Error, LookupAsync for id = {0} should have failed, but succeeded: {1}", id, item);
                    Assert.Fail("");
                }
                catch (InvalidOperationException ex)
                {
                    Log("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                }
            }

            // TODO: Add this test back?
            //#if !WINDOWS_PHONE
            //            // ToCollection - displaying movies on a ListBox
            //            {
            //                var client = this.GetClient();
            //                var table = client.GetTable<StringIdMovie>();
            //                var query = from m in table
            //                            where m.Year > 1980
            //                            orderby m.ReleaseDate descending
            //                            select new
            //                            {
            //                                Date = m.ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            //                                Title = m.Title
            //                            };
            //                query = query.Take(50);
            //                var expectedItems = QueryTestData.AllMovies
            //                    .Where(m => m.Year > 1980)
            //                    .OrderByDescending(m => m.ReleaseDate)
            //                    .Select(m => string.Format(
            //                        "{0} - {1}",
            //                        m.ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            //                        m.Title))
            //                    .Take(50)
            //                    .ToList();
            //                var newPage = new MoviesDisplayControl();
            //                var collection = await query.ToCollectionAsync();
            //                newPage.SetMoviesSource(collection);

            //                Log("Displaying the movie display control with the bound collection");
            //                await newPage.Display();
            //                Log("Dialog displayed, verifying that the items displayed are correct...");
            //                var pageItems = newPage.ItemsAsString;
            //                List<string> errors = new List<string>();
            //                if (Utilities.CompareArrays(expectedItems.ToArray(), pageItems.ToArray(), errors))
            //                {
            //                    Log("Movies were displayed correctly.");
            //                    return;
            //                }
            //                else
            //                {
            //                    Log("Error comparing the movies:");
            //                    foreach (var error in errors)
            //                    {
            //                        Log("  {0}", error);
            //                    }
            //                    Assert.Fail("");
            //                }
            //            }
            //#endif
        }

        class OrderByClause
        {
            public OrderByClause(string fieldName, bool isAscending)
            {
                this.FieldName = fieldName;
                this.IsAscending = isAscending;
            }

            public bool IsAscending { get; private set; }
            public string FieldName { get; private set; }
        }

        private async Task CreateQueryTestIntId(
            string name, Expression<Func<IntIdMovie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<IntIdMovie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, Func<IntIdMovie, bool> whereLambda = null)
        {
            await CreateQueryTest<IntIdMovie, ExceptionTypeWhichWillNeverBeThrown>(
                "[Int id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, false, whereLambda);
        }

        private async Task CreateQueryTestStringId(
            string name, Expression<Func<Movie, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<Movie, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, Func<Movie, bool> whereLambda = null)
        {
            await CreateQueryTest<Movie, ExceptionTypeWhichWillNeverBeThrown>(
                "[String id] " + name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, true, whereLambda);
        }

        private async Task CreateQueryTest<MovieType>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataQueryExpression = null, bool useStringIdTable = false,
            Func<MovieType, bool> whereLambda = null) where MovieType : class, IMovie
        {
            await CreateQueryTest<MovieType, ExceptionTypeWhichWillNeverBeThrown>(name, whereClause, top, skip, orderBy, selectExpression, includeTotalCount, odataQueryExpression, whereLambda: whereLambda);
        }

        private async Task CreateQueryTest<MovieType, TExpectedException>(
            string name, Expression<Func<MovieType, bool>> whereClause,
            int? top = null, int? skip = null, OrderByClause[] orderBy = null,
            Expression<Func<MovieType, string>> selectExpression = null, bool? includeTotalCount = null,
            string odataExpression = null, bool useStringIdTable = false,
            Func<MovieType, bool> whereLambda = null)
            where MovieType : class, IMovie
            where TExpectedException : Exception
        {
            Log("### Executing {0}.", name);
            if (whereClause == null && whereLambda != null)
            {
                Assert.Fail("The argument 'whereLambda' is optional and can only be specified if 'whereClause' is also specified.");
            }

            try
            {
                var table = this.GetClient().GetTable<MovieType>();
                IEnumerable<MovieType> readMovies = null;
                IEnumerable<string> readProjectedMovies = null;

                if (odataExpression == null)
                {
                    IMobileServiceTableQuery<MovieType> query = null;
                    IMobileServiceTableQuery<string> selectedQuery = null;

                    if (whereClause != null)
                    {
                        query = table.Where(whereClause);
                    }

                    if (orderBy != null)
                    {
                        if (query == null)
                        {
                            query = table.Where(m => m.Duration == m.Duration);
                        }

                        query = ApplyOrdering(query, orderBy);
                    }

                    if (top.HasValue)
                    {
                        query = query == null ? table.Take(top.Value) : query.Take(top.Value);
                    }

                    if (skip.HasValue)
                    {
                        query = query == null ? table.Skip(skip.Value) : query.Skip(skip.Value);
                    }

                    if (selectExpression != null)
                    {
                        selectedQuery = query == null ? table.Select(selectExpression) : query.Select(selectExpression);
                    }

                    if (includeTotalCount.HasValue)
                    {
                        query = query.IncludeTotalCount();
                    }

                    if (selectedQuery == null)
                    {
                        // Both ways of querying should be equivalent, so using both with equal probability here.
                        // TODO: Make it deterministic
                        var tickCount = Environment.TickCount;
                        if ((tickCount % 2) == 0)
                        {
                            Log("Querying using MobileServiceTableQuery<T>.ToEnumerableAsync");
                            readMovies = await query.ToEnumerableAsync();
                        }
                        else
                        {
                            Log("Querying using IMobileServiceTable<T>.ReadAsync(MobileServiceTableQuery<U>)");
                            readMovies = await table.ReadAsync(query);
                        }
                    }
                    else
                    {
                        readProjectedMovies = await selectedQuery.ToEnumerableAsync();
                    }
                }
                else
                {
                    Log("Using the OData query directly");
                    JToken result = await table.ReadAsync(odataExpression);
                    readMovies = result.ToObject<IEnumerable<MovieType>>();
                }

                long actualTotalCount = -1;
                ITotalCountProvider totalCountProvider = (readMovies as ITotalCountProvider) ?? (readProjectedMovies as ITotalCountProvider);
                if (totalCountProvider != null)
                {
                    actualTotalCount = totalCountProvider.TotalCount;
                }

                IEnumerable<MovieType> expectedData;
                if (useStringIdTable)
                {
                    var movies = QueryTestData.TestMovies();
                    expectedData = new MovieType[movies.Length];
                    for (var i = 0; i < movies.Length; i++)
                    {
                        ((MovieType[])expectedData)[i] = (MovieType)(IMovie)movies[i];
                    }
                }
                else
                {
                    expectedData = QueryTestData.TestIntIdMovies.Select(s => (MovieType)(IMovie)s);
                }

                // Due to a Xamarin.iOS bug, Expression.Compile() does not work for some expression trees,
                // in which case we allow the caller to provide a lambda directly and we use it instead of
                // compiling the expression tree.
                if (whereLambda != null)
                {
                    expectedData = expectedData.Where(whereLambda);
                }
                else if (whereClause != null)
                {
                    expectedData = expectedData.Where(whereClause.Compile());
                }

                long expectedTotalCount = -1;
                if (includeTotalCount.HasValue && includeTotalCount.Value)
                {
                    expectedTotalCount = expectedData.Count();
                }

                if (orderBy != null)
                {
                    expectedData = ApplyOrdering(expectedData, orderBy);
                }

                if (skip.HasValue)
                {
                    expectedData = expectedData.Skip(skip.Value);
                }

                if (top.HasValue)
                {
                    expectedData = expectedData.Take(top.Value);
                }

                if (includeTotalCount.HasValue)
                {
                    if (expectedTotalCount != actualTotalCount)
                    {
                        Log("Total count was requested, but the returned value is incorrect: expected={0}, actual={1}", expectedTotalCount, actualTotalCount);
                        Assert.Fail("");
                        return;
                    }
                }

                List<string> errors = new List<string>();
                bool expectedDataIsSameAsReadData;

                if (selectExpression != null)
                {
                    string[] expectedProjectedData = expectedData.Select(selectExpression.Compile()).ToArray();
                    expectedDataIsSameAsReadData = Utilities.CompareArrays(expectedProjectedData, readProjectedMovies.ToArray(), errors);
                }
                else
                {
                    expectedDataIsSameAsReadData = Utilities.CompareArrays(expectedData.ToArray(), readMovies.ToArray(), errors);
                }

                if (!expectedDataIsSameAsReadData)
                {
                    foreach (var error in errors)
                    {
                        Log(error);
                    }

                    Log("Expected data is different");
                    Assert.Fail("");
                    return;
                }
                else
                {
                    if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                    {
                        return;
                    }
                    else
                    {
                        Log("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                        Assert.Fail("");
                        return;
                    }
                }
            }
            catch (TExpectedException ex)
            {
                Log("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                return;
            }
        }

        private static IMobileServiceTableQuery<MovieType> ApplyOrdering<MovieType>(IMobileServiceTableQuery<MovieType> query, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return query.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return query.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return query.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return query.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }

        private static IEnumerable<MovieType> ApplyOrdering<MovieType>(IEnumerable<MovieType> data, OrderByClause[] orderBy)
            where MovieType : class, IMovie
        {
            if (orderBy.Length == 1)
            {
                if (orderBy[0].IsAscending && orderBy[0].FieldName == "Title")
                {
                    return data.OrderBy(m => m.Title);
                }
                else if (!orderBy[0].IsAscending && orderBy[0].FieldName == "Year")
                {
                    return data.OrderByDescending(m => m.Year);
                }
            }
            else if (orderBy.Length == 2)
            {
                if (orderBy[1].FieldName == "Title" && orderBy[1].IsAscending)
                {
                    if (orderBy[0].FieldName == "Duration" && orderBy[0].IsAscending)
                    {
                        return data.OrderBy(m => m.Duration).ThenBy(m => m.Title);
                    }
                    else if (orderBy[0].FieldName == "ReleaseDate" && !orderBy[0].IsAscending)
                    {
                        return data.OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Title);
                    }
                }
            }

            throw new NotImplementedException(string.Format("Ordering by [{0}] not implemented yet",
                string.Join(", ", orderBy.Select(c => string.Format("{0} {1}", c.FieldName, c.IsAscending ? "asc" : "desc")))));
        }
    }
}
