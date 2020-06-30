// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataTable("RoundTripTable")]
    public class ToDoWithStringId
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("IntIdRoundTripTable")]
    public class ToDoWithStringIdAgainstIntIdTable
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("IntIdRoundTripTable")]
    public class ToDoWithIntId
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("RoundTripTable")]
    public class RoundTripTableItemWithSystemPropertiesType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }

        [UpdatedAt]
        public DateTimeOffset UpdatedAt { get; set; }

        [Version]
        public String Version { get; set; }
    }

    [Tag("Table")]
    public class MobileServiceTableGenericFunctionalTests : FunctionalTestBase
    {
        private async Task EnsureEmptyTableAsync<T>()
        {
            // Make sure the table is empty
            IMobileServiceTable<T> table = GetClient().GetTable<T>();

            while (true)
            {
                IEnumerable<T> results = await table.Take(1000).ToListAsync();
                T[] items = results.ToArray();

                if (!items.Any())
                {
                    break;
                }

                foreach (T item in items)
                {
                    await table.DeleteAsync(item);
                }
            }
        }

        [AsyncTestMethod]
        private async Task AsyncTableOperationsWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);

                // Read
                IEnumerable<ToDoWithStringId> results = await table.ReadAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].Name);

                // Filter
                results = await table.Where(i => i.Id == testId).ToEnumerableAsync();
                items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].Name);

                // Projection
                var projectedResults = await table.Select(i => new { XId = i.Id, XString = i.Name }).ToEnumerableAsync();
                var projectedItems = projectedResults.ToArray();

                Assert.AreEqual(1, projectedItems.Count());
                Assert.AreEqual(testId, projectedItems[0].XId);
                Assert.AreEqual("Hey", projectedItems[0].XString);

                // Lookup
                item = await table.LookupAsync(testId);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.Name);

                // Update
                item.Name = "What?";
                await table.UpdateAsync(item);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("What?", item.Name);

                // Refresh
                item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.RefreshAsync(item);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("What?", item.Name);

                // Read Again
                results = await table.ReadAsync();
                items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("What?", items[0].Name);

                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task OrderingReadAsyncWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = new string[] { "a", "b", "C", "_A", "_B", "_C", "1", "2", "3" };
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            IEnumerable<ToDoWithStringId> results = await table.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringId[] items = results.ToArray();

            Assert.AreEqual(9, items.Count());
            Assert.AreEqual("_A", items[0].Id);
            Assert.AreEqual("_B", items[1].Id);
            Assert.AreEqual("_C", items[2].Id);
            Assert.AreEqual("1", items[3].Id);
            Assert.AreEqual("2", items[4].Id);
            Assert.AreEqual("3", items[5].Id);
            Assert.AreEqual("a", items[6].Id);
            Assert.AreEqual("b", items[7].Id);
            Assert.AreEqual("C", items[8].Id);

            results = await table.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(9, items.Count());
            Assert.AreEqual("_A", items[8].Id);
            Assert.AreEqual("_B", items[7].Id);
            Assert.AreEqual("_C", items[6].Id);
            Assert.AreEqual("1", items[5].Id);
            Assert.AreEqual("2", items[4].Id);
            Assert.AreEqual("3", items[3].Id);
            Assert.AreEqual("a", items[2].Id);
            Assert.AreEqual("b", items[1].Id);
            Assert.AreEqual("C", items[0].Id);

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            string[] invalidIdData = IdTestData.EmptyStringIds.Concat(
                                    IdTestData.InvalidStringIds).Concat(
                                    new string[] { null }).ToArray();

            foreach (string invalidId in invalidIdData)
            {
                IEnumerable<ToDoWithStringId> results = await table.Where(p => p.Id == invalidId).ToEnumerableAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.AreEqual(0, items.Count());
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.LookupAsync(testId);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message == "The item does not exist" ||
                              exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithNoSuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                InvalidOperationException exception = null;
                try
                {
                    await table.RefreshAsync(item);
                }
                catch (InvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            string[] emptyIdData = IdTestData.EmptyStringIds.Concat(
                                    new string[] { null }).ToArray();
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            int count = 0;
            List<ToDoWithStringId> itemsToDelete = new List<ToDoWithStringId>();

            foreach (string emptyId in emptyIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = emptyId, Name = (++count).ToString() };
                await table.InsertAsync(item);

                Assert.IsNotNull(item.Id);
                Assert.AreEqual(count.ToString(), item.Name);
                itemsToDelete.Add(item);
            }

            foreach (var item in itemsToDelete)
            {
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithExistingItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                item.Name = "No we're talking!";

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.InsertAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.Conflict);
                Assert.IsTrue(exception.Message.Contains("Could not insert the item because an item with that id already exists.") ||
                              exception.Message == "The request could not be completed.  (Conflict)");
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;
                item.Name = "Alright!";

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.UpdateAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message == "The item does not exist" ||
                              exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.DeleteAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message == "The item does not exist" ||
                              exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_ThrowsPreconditionFailedException_WhenMergeConflictOccurs()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();
            string id = Guid.NewGuid().ToString();
            IMobileServiceTable table = GetClient().GetTable("RoundTripTable");

            var item = new JObject() { { "id", id }, { "name", "a value" } };
            var inserted = await table.InsertAsync(item);
            item["version"] = "3q3A3g==";

            MobileServicePreconditionFailedException expectedException = null;
            try
            {
                await table.DeleteAsync(item);
            }
            catch (MobileServicePreconditionFailedException ex)
            {
                expectedException = ex;
            }
            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Value["version"], inserted["version"]);
            Assert.AreEqual(expectedException.Value["name"], inserted["name"]);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_ThrowsPreconditionFailedException_WhenMergeConflictOccurs_Generic()
        {
            string id = Guid.NewGuid().ToString();
            var table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            // insert a new item
            var item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            string version = item.Version;

            // Delete with wrong version
            item.Version = "3q3A3g==";
            item.Name = "But wait!";
            MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType> expectedException = null;
            try
            {
                await table.DeleteAsync(item);
            }
            catch (MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType> exception)
            {
                expectedException = exception;
            }

            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Response.StatusCode, HttpStatusCode.PreconditionFailed);

            string responseContent = await expectedException.Response.Content.ReadAsStringAsync();

            RoundTripTableItemWithSystemPropertiesType serverItem = expectedException.Item;
            string serverVersion = serverItem.Version;
            string stringValue = serverItem.Name;

            Assert.AreEqual(version, serverVersion);
            Assert.AreEqual(stringValue, "a value");

            Assert.IsNotNull(expectedException.Item);
            Assert.AreEqual(version, expectedException.Item.Version);
            Assert.AreEqual(stringValue, expectedException.Item.Name);

            // Delete one last time with the version from the server
            item.Version = serverVersion;
            await table.DeleteAsync(item);

            Assert.IsNull(item.Id);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithIntegerAsStringIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringIdAgainstIntIdTable>();

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();
            ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Name = "Hey" };

            // Insert
            await stringIdTable.InsertAsync(item);
            string testId = item.Id.ToString();

            // Read
            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.ReadAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("Hey", items[0].Name);

            // Filter
            results = await stringIdTable.Where(i => i.Id == testId).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("Hey", items[0].Name);

            // Projection
            var projectedResults = await stringIdTable.Select(i => new { XId = i.Id, XString = i.Name }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.AreEqual(1, projectedItems.Count());
            Assert.AreEqual(testId, projectedItems[0].XId);
            Assert.AreEqual("Hey", projectedItems[0].XString);

            // Lookup
            ToDoWithStringIdAgainstIntIdTable stringIdItem = await stringIdTable.LookupAsync(testId);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("Hey", stringIdItem.Name);

            // Update
            stringIdItem.Name = "What?";
            await stringIdTable.UpdateAsync(stringIdItem);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("What?", stringIdItem.Name);

            // Refresh
            stringIdItem = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, Name = "Hey" };
            await stringIdTable.RefreshAsync(stringIdItem);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("What?", stringIdItem.Name);

            // Read Again
            results = await stringIdTable.ReadAsync();
            items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("What?", items[0].Name);

            // Delete
            await stringIdTable.DeleteAsync(item);
        }

        [AsyncTestMethod]
        [Tag("NodeRuntimeOnly")]
        public async Task AsyncTableOperationsWithStringIdAgainstIntegerIdTable()
        {
            Log("This test fails with the .NET backend since in .NET the DTO always has string-id. In Node, querying an int-id column for a string causes an error.");

            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            foreach (string testId in testIdData)
            {
                // Filter
                MobileServiceInvalidOperationException exception = null;
                try
                {
                    IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == testId).ToEnumerableAsync();
                    ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.Equals(exception.Response.StatusCode, HttpStatusCode.BadRequest);

                // Refresh
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, Name = "Hey!" };
                    await stringIdTable.RefreshAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.Equals(exception.Response.StatusCode, HttpStatusCode.BadRequest);

                // Lookup
                exception = null;
                try
                {
                    await stringIdTable.LookupAsync(testId);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.Equals(exception.Response.StatusCode, HttpStatusCode.BadRequest);

                // Update
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, Name = "Hey!" };
                    await stringIdTable.UpdateAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.Equals(exception.Response.StatusCode, HttpStatusCode.BadRequest);

                // Delete
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, Name = "Hey!" };
                    await stringIdTable.DeleteAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.Equals(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task OrderingReadAsyncWithStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.AreEqual(10, items.Count());
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual((int.Parse(items[i].Id) + 1).ToString(), items[i + 1].Id);
            }

            results = await stringIdTable.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(10, items.Count());
            for (var i = 8; i >= 0; i--)
            {
                Assert.AreEqual((int.Parse(items[i].Id) - 1).ToString(), items[i + 1].Id);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithIntegerAsStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == integerIdItems[0].Id.ToString()).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(integerIdItems[0].Id.ToString(), items[0].Id);
            Assert.AreEqual("0", items[0].Name);

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            string[] testIdData = new string[] { "", " ", null };

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            foreach (string testId in testIdData)
            {
                IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == testId).ToEnumerableAsync();
                ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

                Assert.AreEqual(0, items.Length);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithValidIntIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();

            ToDoWithIntId item = new ToDoWithIntId() { Name = "Hey" };
            await table.InsertAsync(item);

            IEnumerable<ToDoWithIntId> results = await table.ReadAsync();
            ToDoWithIntId[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.IsTrue(items[0].Id > 0);
            Assert.AreEqual("Hey", items[0].Name);

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithAllSystemProperties()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            string id = Guid.NewGuid().ToString();
            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            // Read
            IEnumerable<RoundTripTableItemWithSystemPropertiesType> results = await table.ReadAsync();
            RoundTripTableItemWithSystemPropertiesType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.IsNotNull(items[0].CreatedAt);
            Assert.IsNotNull(items[0].UpdatedAt);
            Assert.IsNotNull(items[0].Version);

            // Filter against version
            // BUG #1706815 (OData query for version field (string <--> byte[] mismatch)
            /*
            results = await table.Where(i => i.Version == items[0].Version).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);

            // Filter against createdAt
            results = await table.Where(i => i.CreatedAt == items[0].CreatedAt).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);

            // Filter against updatedAt
            results = await table.Where(i => i.UpdatedAt == items[0].UpdatedAt).ToEnumerableAsync();
            filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);
            */

            // Projection
            var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.AreEqual(1, projectedResults.Count());
            Assert.AreEqual(projectedItems[0].XId, items[0].Id);
            Assert.AreEqual(projectedItems[0].XCreatedAt, items[0].CreatedAt);
            Assert.AreEqual(projectedItems[0].XUpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(projectedItems[0].XVersion, items[0].Version);

            // Lookup
            item = await table.LookupAsync(id);
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            // Refresh
            item = new RoundTripTableItemWithSystemPropertiesType() { Id = id };
            await table.RefreshAsync(item);
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            // Update
            item.Name = "Hello!";
            await table.UpdateAsync(item);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.IsTrue(item.UpdatedAt >= items[0].UpdatedAt);
            Assert.IsNotNull(item.Version);
            Assert.AreNotEqual(item.Version, items[0].Version);

            // Read Again
            results = await table.ReadAsync();
            items = results.ToArray();
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithSystemPropertiesSetExplicitly()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> allSystemPropertiesTable = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            // Regular insert
            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Name = "a value" };
            await allSystemPropertiesTable.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            // Explicit System Properties Read
            IEnumerable<RoundTripTableItemWithSystemPropertiesType> results = await allSystemPropertiesTable.Where(p => p.Id == item.Id).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.IsNotNull(items[0].CreatedAt);
            Assert.IsNotNull(items[0].UpdatedAt);
            Assert.IsNotNull(items[0].Version);

            // Lookup
            var item3 = await allSystemPropertiesTable.LookupAsync(item.Id);
            Assert.AreEqual(item.CreatedAt, item3.CreatedAt);
            Assert.AreEqual(item.UpdatedAt, item3.UpdatedAt);
            Assert.IsNotNull(item3.Version);

            await allSystemPropertiesTable.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();

            // Insert some items
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };

                await table.InsertAsync(item);

                Assert.IsNotNull(item.CreatedAt);
                Assert.IsNotNull(item.UpdatedAt);
                Assert.IsNotNull(item.Version);
                items.Add(item);
            }

            // Ordering
            var results = await table.OrderBy(t => t.CreatedAt).ToEnumerableAsync(); // Fails here with .NET runtime. Why??
            RoundTripTableItemWithSystemPropertiesType[] orderItems = results.ToArray();

            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            results = await table.OrderBy(t => t.UpdatedAt).ToEnumerableAsync();
            orderItems = results.ToArray();

            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            results = await table.OrderBy(t => t.Version).ToEnumerableAsync();
            orderItems = results.ToArray();

            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            // Filtering
            results = await table.Where(t => t.CreatedAt >= items[4].CreatedAt).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] filteredItems = results.ToArray();

            for (int i = 0; i < filteredItems.Length - 1; i++)
            {
                Assert.IsTrue(filteredItems[i].CreatedAt >= items[4].CreatedAt);
            }

            results = await table.Where(t => t.UpdatedAt >= items[4].UpdatedAt).ToEnumerableAsync();
            filteredItems = results.ToArray();

            for (int i = 0; i < filteredItems.Length - 1; i++)
            {
                Assert.IsTrue(filteredItems[i].UpdatedAt >= items[4].UpdatedAt);
            }

            // TODO: Seperate to own test, to not run for .NET / Fix.Net
            /*
            results = await table.Where(t => t.Version == items[4].Version).ToEnumerableAsync();
            filteredItems = results.ToArray();

            for (int i = 0; i < filteredItems.Length - 1; i++)
            {
                Assert.IsTrue(filteredItems[i].Version == items[4].Version);
            }
            */

            // Selection
            var selectionResults = await table.Select(t => new { Id = t.Id, CreatedAt = t.CreatedAt }).ToEnumerableAsync();
            var selectedItems = selectionResults.ToArray();

            for (int i = 0; i < selectedItems.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems[i].Id).FirstOrDefault();
                Assert.IsTrue(item.CreatedAt == selectedItems[i].CreatedAt);
            }

            var selectionResults2 = await table.Select(t => new { Id = t.Id, UpdatedAt = t.UpdatedAt }).ToEnumerableAsync();
            var selectedItems2 = selectionResults2.ToArray();

            for (int i = 0; i < selectedItems2.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems2[i].Id).FirstOrDefault();
                Assert.IsTrue(item.UpdatedAt == selectedItems2[i].UpdatedAt);
            }

            var selectionResults3 = await table.Select(t => new { Id = t.Id, Version = t.Version }).ToEnumerableAsync();
            var selectedItems3 = selectionResults3.ToArray();

            for (int i = 0; i < selectedItems3.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems3[i].Id).FirstOrDefault();
                Assert.IsTrue(item.Version == selectedItems3[i].Version);
            }

            // Delete
            foreach (var item in items)
            {
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithMergeConflict()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();
            string id = Guid.NewGuid().ToString();
            IMobileServiceTable table = GetClient().GetTable("RoundTripTable");

            var item = new JObject() { { "id", id }, { "name", "a value" } };
            var inserted = await table.InsertAsync(item);
            item["version"] = "3q3A3g==";

            MobileServicePreconditionFailedException expectedException = null;
            try
            {
                await table.UpdateAsync(item);
            }
            catch (MobileServicePreconditionFailedException ex)
            {
                expectedException = ex;
            }
            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Value["version"], inserted["version"]);
            Assert.AreEqual(expectedException.Value["name"], inserted["name"]);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWitMergeConflict_Generic()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            string id = Guid.NewGuid().ToString();
            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            string version = item.Version;

            // Update
            item.Name = "Hello!";
            await table.UpdateAsync(item);
            Assert.IsNotNull(item.Version);
            Assert.AreNotEqual(item.Version, version);

            string newVersion = item.Version;

            // Update again but with the original version
            item.Version = version;
            item.Name = "But wait!";
            MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType> expectedException = null;
            try
            {
                await table.UpdateAsync(item);
            }
            catch (MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType> exception)
            {
                expectedException = exception;
            }

            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Response.StatusCode, HttpStatusCode.PreconditionFailed);

            Assert.IsNotNull(expectedException.Item);

            string serverVersion = expectedException.Item.Version;
            string stringValue = expectedException.Item.Name;

            Assert.AreEqual(newVersion, serverVersion);
            Assert.AreEqual(stringValue, "Hello!");

            // Update one last time with the version from the server
            item.Version = serverVersion;
            await table.UpdateAsync(item);
            Assert.IsNotNull(item.Version);
            Assert.AreEqual(item.Name, "But wait!");
            Assert.AreNotEqual(item.Version, serverVersion);

            await table.DeleteAsync(item);
        }
    }
}
