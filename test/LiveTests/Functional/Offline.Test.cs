// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileTest.Framework;
using MobileTest.LiveTests.Base;
using MobileTest.LiveTests.TestData;
using Newtonsoft.Json.Linq;

namespace MobileTest.LiveTests.Functional
{
    [Tag("offline")]
    public class OfflineTests : FunctionalTestBase
    {
        public static string StoreFileName = "store.bin";

        [AsyncTestMethod]
        private async Task BasicOfflineTest()
        {
            ClearStore();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using random seed: {0}", seed);
            Random rndGen = new Random(seed);

            CountingHandler handler = new CountingHandler();
            var requestsSentToServer = 0;
            var offlineReadyClient = CreateClient(handler);

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            Log("Defined the table on the local store");
            localStore.DefineTable<OfflineReadyItem>();

            await offlineReadyClient.SyncContext.InitializeAsync(localStore);
            Log("Initialized the store and sync context");

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

            var item = new OfflineReadyItem(rndGen);
            try
            {
                await localTable.InsertAsync(item);
                Log("Inserted the item to the local store:", item);

                Log("Validating that the item is not in the server table");
                try
                {
                    requestsSentToServer++;
                    await remoteTable.LookupAsync(item.Id);
                    Assert.Fail("Error, item is present in the server");
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    Log("Ok, item is not in the server: {0}", ex.Message);
                }

                Func<int, bool> validateRequestCount = expectedCount =>
                {
                    Log("So far {0} requests sent to the server", handler.RequestCount);
                    if (handler.RequestCount != expectedCount)
                    {
                        Log("Error, expected {0} requests to have been sent to the server", expectedCount);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                };

                if (!validateRequestCount(requestsSentToServer))
                {
                    Assert.Fail(string.Format("Error, expected {0} requests to have been sent to the server", requestsSentToServer));
                }
                Log("Pushing changes to the server");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer++;

                if (!validateRequestCount(requestsSentToServer))
                {
                    Assert.Fail(string.Format("Error, expected {0} requests to have been sent to the server", requestsSentToServer));
                }

                Log("Push done; now verifying that item is in the server");

                var serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                Log("Retrieved item from server: {0}", serverItem);
                if (serverItem.Equals(item))
                {
                    Log("Items are the same");
                }
                else
                {
                    Assert.Fail(string.Format("Items are different. Local: {0}; remote: {1}", item, serverItem));
                }

                Log("Now updating the item locally");
                item.Flag = !item.Flag;
                item.Age++;
                item.Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, DateTimeKind.Utc);
                await localTable.UpdateAsync(item);
                Log("Item has been updated");

                var newItem = new OfflineReadyItem(rndGen);
                Log("Adding a new item to the local table: {0}", newItem);
                await localTable.InsertAsync(newItem);

                if (!validateRequestCount(requestsSentToServer))
                {
                    Assert.Fail(string.Format("Error, expected {0} requests to have been sent to the server", requestsSentToServer));
                }

                Log("Pushing the new changes to the server");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;

                if (!validateRequestCount(requestsSentToServer))
                {
                    Assert.Fail(string.Format("Error, expected {0} requests to have been sent to the server", requestsSentToServer));
                }

                Log("Push done. Verifying changes on the server");
                serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                if (serverItem.Equals(item))
                {
                    Log("Updated items are the same");
                }
                else
                {
                    Assert.Fail(string.Format("Items are different. Local: {0}; remote: {1}", item, serverItem));
                }

                serverItem = await remoteTable.LookupAsync(newItem.Id);
                requestsSentToServer++;
                if (serverItem.Equals(newItem))
                {
                    Log("New inserted item is the same");
                }
                else
                {
                    Assert.Fail(string.Format("Items are different. Local: {0}; remote: {1}", item, serverItem));
                }

                Log("Cleaning up");
                await localTable.DeleteAsync(item);
                await localTable.DeleteAsync(newItem);
                Log("Local table cleaned up. Now sync'ing once more");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;
                if (!validateRequestCount(requestsSentToServer))
                {
                    Assert.Fail(string.Format("Error, expected {0} requests to have been sent to the server", requestsSentToServer));
                }
                Log("Done");
            }
            catch (MobileServicePushFailedException ex)
            {
                Log("Push Result status from MobileServicePushFailedException: " + ex.PushResult.Status);
                throw;
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
        }

        [AsyncTestMethod]
        private async Task ClientResolvesConflictsTest()
        {
            await CreateSyncConflict(true);
        }

        [AsyncTestMethod]
        private async Task PushFailsAfterConflictsTest()
        {
            await CreateSyncConflict(false);
        }

        [AsyncTestMethod]
        private async Task AbortPushAtStartSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.Start);
        }

        [AsyncTestMethod]
        private async Task AbortPushAtMiddleSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.Middle);
        }

        [AsyncTestMethod]
        private async Task AbortPushAtEndSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.End);
        }

        [AsyncTestMethod]
        private async Task AuthenticatedTableSyncTest()
        {
            bool isUserLoggedIn = false;
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using random seed: {0}", seed);
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            Log("Defined the table on the local store");
            localStore.DefineTable<OfflineReadyItemNoVersion>();

            await offlineReadyClient.SyncContext.InitializeAsync(localStore);
            Log("Initialized the store and sync context");

            try
            {
                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItemNoVersion>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItemNoVersion>();

                var item = new OfflineReadyItemNoVersion(rndGen);
                await localTable.InsertAsync(item);
                Log("Inserted the item to the local store:", item);

                try
                {
                    await offlineReadyClient.SyncContext.PushAsync();
                    Log("Pushed the changes to the server");
                    if (isUserLoggedIn)
                    {
                        Log("As expected, push succeeded");
                    }
                    else
                    {
                        Assert.Fail("Error, table should only work with authenticated access, but user is not logged in");
                    }
                }
                catch (MobileServicePushFailedException ex)
                {
                    if (isUserLoggedIn)
                    {
                        Assert.Fail(string.Format("Error, user is logged in but push operation failed: {0}", ex));
                    }

                    Log("Got expected exception: {0}: {1}", ex.GetType().FullName, ex.Message);
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        Log("  {0}: {1}", inner.GetType().FullName, inner.Message);
                        inner = inner.InnerException;
                    }
                }

                if (!isUserLoggedIn)
                {
                    Log("Push should have failed, so now will try to log in to complete the push operation");
                    MobileServiceUser user = await Utilities.GetDummyUser(offlineReadyClient);
                    offlineReadyClient.CurrentUser = user;
                    Log("Logged in as {0}", offlineReadyClient.CurrentUser.UserId);
                    await offlineReadyClient.SyncContext.PushAsync();
                    Log("Push succeeded");
                }

                await localTable.PurgeAsync();
                Log("Purged the local table");
                await localTable.PullAsync(null, localTable.Where(i => i.Id == item.Id));
                Log("Pulled the data into the local table");
                List<OfflineReadyItemNoVersion> serverItems = await localTable.ToListAsync();
                Log("Retrieved items from the local table");

                Log("Removing item from the remote table");
                await remoteTable.DeleteAsync(item);

                if (!isUserLoggedIn)
                {
                    await offlineReadyClient.LogoutAsync();
                    Log("Logged out again");
                }

                var firstServerItem = serverItems.FirstOrDefault();
                if (item.Equals(firstServerItem))
                {
                    Log("Data round-tripped successfully");
                }
                else
                {
                    Assert.Fail(string.Format("Error, data did not round-trip successfully. Expected: {0}, actual: {1}", item, firstServerItem));
                }

                Log("Cleaning up");
                await localTable.PurgeAsync();
                Log("Done");
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
            await offlineReadyClient.LogoutAsync();
        }

        [AsyncTestMethod]
        private async Task NoOptimisticConcurrencyTest()
        {
            // If a table does not have a version column, then offline will still
            // work, but there will be no conflicts
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using random seed: {0}", seed);
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            Log("Defined the table on the local store");
            localStore.DefineTable<OfflineReadyItemNoVersion>();

            await offlineReadyClient.SyncContext.InitializeAsync(localStore);
            Log("Initialized the store and sync context");

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItemNoVersion>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItemNoVersion>();

            var item = new OfflineReadyItemNoVersion(rndGen);
            try
            {
                offlineReadyClient.CurrentUser = await Utilities.GetDummyUser(offlineReadyClient);
                await localTable.InsertAsync(item);
                Log("Inserted the item to the local store:", item);
                await offlineReadyClient.SyncContext.PushAsync();

                Log("Pushed the changes to the server");

                var serverItem = await remoteTable.LookupAsync(item.Id);
                serverItem.Name = "changed name";
                serverItem.Age = 0;
                await remoteTable.UpdateAsync(serverItem);
                Log("Server item updated (changes will be overwritten later");

                item.Age = item.Age + 1;
                item.Name = item.Name + " - modified";
                await localTable.UpdateAsync(item);
                Log("Updated item locally, will now push changes to the server: {0}", item);
                await offlineReadyClient.SyncContext.PushAsync();

                serverItem = await remoteTable.LookupAsync(item.Id);
                Log("Retrieved the item from the server: {0}", serverItem);

                if (serverItem.Equals(item))
                {
                    Log("Items are the same");
                }
                else
                {
                    Assert.Fail(string.Format("Items are different. Local: {0}; remote: {1}", item, serverItem));
                }

                Log("Cleaning up");
                localTable.DeleteAsync(item).Wait();
                Log("Local table cleaned up. Now sync'ing once more");
                await offlineReadyClient.SyncContext.PushAsync();
            }

            catch (MobileServicePushFailedException ex)
            {
                Log("PushResult status: " + ex.PushResult.Status);
                throw;
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
            await offlineReadyClient.LogoutAsync();
        }

        private MobileServiceClient CreateClient(params HttpMessageHandler[] handlers)
        {
            var globalClient = GetClient();
            var offlineReadyClient = new MobileServiceClient(
                globalClient.MobileAppUri,
                handlers);

            if (globalClient.CurrentUser != null)
            {
                offlineReadyClient.CurrentUser = new MobileServiceUser(globalClient.CurrentUser.UserId);
                offlineReadyClient.CurrentUser.MobileServiceAuthenticationToken = globalClient.CurrentUser.MobileServiceAuthenticationToken;
            }

            return offlineReadyClient;
        }

        enum SyncAbortLocation { Start, Middle, End };

        class AbortingSyncHandler : IMobileServiceSyncHandler
        {
            OfflineTests test;

            public AbortingSyncHandler(OfflineTests offlineTest, Func<string, bool> shouldAbortForId)
            {
                this.test = offlineTest;
                this.AbortCondition = shouldAbortForId;
            }

            public Func<string, bool> AbortCondition { get; set; }

            public Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                var itemId = (string)operation.Item[MobileServiceSystemColumns.Id];
                if (this.AbortCondition(itemId))
                {
                    this.test.Log("Found id to abort ({0}), aborting the push operation", itemId);
                    operation.AbortPush();
                }
                else
                {
                    this.test.Log("Pushing operation {0} for item {1}", operation.Kind, itemId);
                }

                return operation.ExecuteAsync();
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private async Task AbortPushDuringSync(SyncAbortLocation whereToAbort)
        {
            ClearStore();
            SyncAbortLocation abortLocation = whereToAbort;
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using random seed: {0}", seed);
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var items = Enumerable.Range(0, 10).Select(_ => new OfflineReadyItem(rndGen)).ToArray();
            foreach (var item in items)
            {
                item.Id = Guid.NewGuid().ToString("D");
            }

            int abortIndex = abortLocation == SyncAbortLocation.Start ? 0 :
                (abortLocation == SyncAbortLocation.End ? items.Length - 1 : rndGen.Next(1, items.Length - 1));
            var idToAbort = items[abortIndex].Id;
            Log("Will send {0} items, aborting when id = {1}", items.Length, idToAbort);

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            Log("Defined the table on the local store");
            localStore.DefineTable<OfflineReadyItem>();

            var syncHandler = new AbortingSyncHandler(this, id => id == idToAbort);
            await offlineReadyClient.SyncContext.InitializeAsync(localStore, syncHandler);
            Log("Initialized the store and sync context");

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();
            try
            {
                foreach (var item in items)
                {
                    await localTable.InsertAsync(item);
                }

                Log("Inserted {0} items in the local table. Now pushing those", items.Length);

                try
                {
                    await offlineReadyClient.SyncContext.PushAsync();
                    Assert.Fail("Error, push call should have failed");
                }
                catch (MobileServicePushFailedException ex)
                {
                    Log("Caught (expected) exception: {0}", ex);
                }

                var expectedOperationQueueSize = items.Length - abortIndex;
                Log("Current operation queue size: {0}", offlineReadyClient.SyncContext.PendingOperations);
                if (expectedOperationQueueSize != offlineReadyClient.SyncContext.PendingOperations)
                {
                    Assert.Fail(string.Format("Error, expected {0} items in the queue", expectedOperationQueueSize));
                }

                foreach (var allItemsPushed in new bool[] { false, true })
                {
                    HashSet<OfflineReadyItem> itemsInServer, itemsNotInServer;
                    if (allItemsPushed)
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.ToArray());
                        itemsNotInServer = new HashSet<OfflineReadyItem>(Enumerable.Empty<OfflineReadyItem>());
                    }
                    else
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index < abortIndex));
                        itemsNotInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index >= abortIndex));
                    }

                    foreach (var item in items)
                    {
                        var itemFromServer = (await remoteTable.Where(i => i.Id == item.Id).Take(1).ToEnumerableAsync()).FirstOrDefault();
                        Log("Item with id = {0} from server: {1}", item.Id,
                            itemFromServer == null ? "<<null>>" : itemFromServer.ToString());
                        if (itemsInServer.Contains(item) && itemFromServer == null)
                        {
                            Assert.Fail(string.Format("Error, the item {0} should have made to the server", item.Id));
                        }
                        else if (itemsNotInServer.Contains(item) && itemFromServer != null)
                        {
                            Assert.Fail(string.Format("Error, the item {0} should not have made to the server", item.Id));
                        }
                    }

                    if (!allItemsPushed)
                    {
                        Log("Changing the handler so that it doesn't abort anymore.");
                        syncHandler.AbortCondition = _ => false;
                        Log("Pushing again");
                        await offlineReadyClient.SyncContext.PushAsync();
                        Log("Finished pushing all elements");
                    }
                }

                Log("Changing the handler so that it doesn't abort anymore.");
                syncHandler.AbortCondition = _ => false;

                Log("Cleaning up");
                foreach (var item in items)
                {
                    await localTable.DeleteAsync(item);
                }

                await offlineReadyClient.SyncContext.PushAsync();
                Log("Done");
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
        }

        class CountingHandler : DelegatingHandler
        {
            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestCount++;
                return base.SendAsync(request, cancellationToken);
            }
        }

        class ConflictResolvingSyncHandler<T> : IMobileServiceSyncHandler
        {
            public delegate T ConflictResolution(T clientItem, T serverItem);
            IMobileServiceClient client;
            ConflictResolution conflictResolution;
            OfflineTests test;

            public ConflictResolvingSyncHandler(OfflineTests offlineTest, IMobileServiceClient client, ConflictResolution resolutionPolicy)
            {
                this.client = client;
                this.conflictResolution = resolutionPolicy;
                this.test = offlineTest;
            }

            public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                MobileServicePreconditionFailedException ex = null;
                JObject result = null;
                do
                {
                    ex = null;
                    try
                    {
                        this.test.Log("Attempting to execute the operation");
                        result = await operation.ExecuteAsync();
                    }
                    catch (MobileServicePreconditionFailedException e)
                    {
                        ex = e;
                    }

                    if (ex != null)
                    {
                        this.test.Log("A MobileServicePreconditionFailedException was thrown, ex.Value = {0}", ex.Value);
                        var serverItem = ex.Value;
                        if (serverItem == null)
                        {
                            this.test.Log("Item not returned in the exception, trying to retrieve it from the server");
                            serverItem = (JObject)(await client.GetTable(operation.Table.TableName).LookupAsync((string)operation.Item["id"]));
                        }

                        var typedClientItem = operation.Item.ToObject<T>();
                        var typedServerItem = serverItem.ToObject<T>();
                        var typedMergedItem = conflictResolution(typedClientItem, typedServerItem);
                        var mergedItem = JObject.FromObject(typedMergedItem);
                        mergedItem[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];
                        this.test.Log("Merged the items, will try to resubmit the operation");
                        operation.Item = mergedItem;
                    }
                } while (ex != null);

                return result;
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private async Task CreateSyncConflict(bool autoResolve)
        {
            ClearStore();
            bool resolveConflictsOnClient = autoResolve;
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using random seed: {0}", seed);
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            Log("Defined the table on the local store");
            localStore.DefineTable<OfflineReadyItem>();

            ConflictResolvingSyncHandler<OfflineReadyItem>.ConflictResolution conflictHandlingPolicy;
            conflictHandlingPolicy = (client, server) =>
                    new OfflineReadyItem
                    {
                        Id = client.Id,
                        Age = Math.Max(client.Age, server.Age),
                        Date = client.Date > server.Date ? client.Date : server.Date,
                        Flag = client.Flag || server.Flag,
                        FloatingNumber = Math.Max(client.FloatingNumber, server.FloatingNumber),
                        Name = client.Name
                    };
            if (resolveConflictsOnClient)
            {
                var handler = new ConflictResolvingSyncHandler<OfflineReadyItem>(this, offlineReadyClient, conflictHandlingPolicy);
                await offlineReadyClient.SyncContext.InitializeAsync(localStore, handler);
            }
            else
            {
                await offlineReadyClient.SyncContext.InitializeAsync(localStore);
            }

            Log("Initialized the store and sync context");

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

            await localTable.PurgeAsync();
            Log("Removed all items from the local table");

            var item = new OfflineReadyItem(rndGen);
            await remoteTable.InsertAsync(item);
            Log("Inserted the item to the remote store:", item);

            var pullQuery = "$filter=id eq '" + item.Id + "'";
            await localTable.PullAsync(null, pullQuery);

            Log("Changing the item on the server");
            item.Age++;
            await remoteTable.UpdateAsync(item);
            Log("Updated the item: {0}", item);

            var localItem = await localTable.LookupAsync(item.Id);
            Log("Retrieved the item from the local table, now updating it");
            localItem.Date = localItem.Date.AddDays(1);
            await localTable.UpdateAsync(localItem);
            Log("Updated the item on the local table");

            Log("Now trying to pull changes from the server (will trigger a push)");
            string errorMessage = string.Empty;
            try
            {
                await localTable.PullAsync(null, pullQuery);
                if (!autoResolve)
                {
                    errorMessage = "Error, pull (push) should have caused a conflict, but none happened.";
                }
                else
                {
                    var expectedMergedItem = conflictHandlingPolicy(localItem, item);
                    var localMergedItem = await localTable.LookupAsync(item.Id);
                    if (localMergedItem.Equals(expectedMergedItem))
                    {
                        Log("Item was merged correctly.");
                    }
                    else
                    {
                        errorMessage = string.Format("Error, item not merged correctly. Expected: {0}, Actual: {1}", expectedMergedItem, localMergedItem);
                    }
                }
            }
            catch (MobileServicePushFailedException ex)
            {
                Log("Push exception: {0}", ex);
                if (autoResolve)
                {
                    errorMessage = "Error, push should have succeeded.";
                }
                else
                {
                    Log("Expected exception was thrown.");
                }
            }

            Log("Cleaning up");
            await localTable.DeleteAsync(item);
            Log("Local table cleaned up. Now sync'ing once more");
            await offlineReadyClient.SyncContext.PushAsync();
            Log("Done");
            localStore.Dispose();
            ClearStore();
            if (!String.IsNullOrEmpty(errorMessage))
            {
                Assert.Fail(errorMessage);
            }
        }

        private static void ClearStore()
        {
            File.Delete(StoreFileName);
        }
    }
}
