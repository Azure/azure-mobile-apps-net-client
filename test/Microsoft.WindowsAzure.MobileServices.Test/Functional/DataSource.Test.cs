// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    [TestCategory("live-tests")]
    [TestClass, Ignore]
    public class DataSource_Test : FunctionalTestBase
    {
        [TestMethod]
        public async Task SimpleDataSource()
        {
            // Get the Books table
            IMobileServiceTable<Book> table = GetClient().GetTable<Book>();

            // Create a new CollectionView
            MobileServiceCollection<Book, Book> dataSource =
                await table.OrderByDescending(b => b.Price).ToCollectionAsync();

            Assert.AreEqual(18, dataSource.Count);
            Assert.AreEqual((long)-1, ((ITotalCountProvider)dataSource).TotalCount);
            Assert.AreEqual(22.95, dataSource[0].Price);
        }

        [TestMethod]
        public async Task SimpleDataSourceWithTotalCount()
        {
            // Get the Books table
            IMobileServiceTable<Book> table = GetClient().GetTable<Book>();

            // Create a new CollectionView
            MobileServiceCollection<Book, Book> dataSource =
                await table.Take(5).IncludeTotalCount().ToCollectionAsync();

            Assert.AreEqual(5, dataSource.Count);
            Assert.AreEqual((long)18, ((ITotalCountProvider)dataSource).TotalCount);
        }
    }
}
