// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("Misc")]
    public class MiscTests : FunctionalTestBase
    {
        [DataTable("ParamsTestTable")]
        public class ParamsTestTableItem
        {
            public int Id { get; set; }
            public string parameters { get; set; }
        }

        [DataTable("RoundTripTable")]
        class VersionedType
        {
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "number")]
            public double Number { get; set; }

            [Version]
            public string Version { get; set; }

            [CreatedAt]
            public DateTime CreatedAt { get; set; }

            [UpdatedAt]
            public DateTime UpdatedAt { get; set; }

            public VersionedType() { }
            public VersionedType(Random rndGen)
            {
                this.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
                this.Number = rndGen.Next(10000);
            }

            private VersionedType(VersionedType other)
            {
                this.Id = other.Id;
                this.Name = other.Name;
                this.Number = other.Number;
                this.Version = other.Version;
                this.CreatedAt = other.CreatedAt;
                this.UpdatedAt = other.UpdatedAt;
            }

            public override string ToString()
            {
                return string.Format("Versioned[Id={0},Name={1},Number={2},Version={3},CreatedAt={4},UpdatedAt={5}]",
                    Id, Name, Number, Version,
                    CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                    UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
            }

            public override int GetHashCode()
            {
                int result = 0;
                if (Name != null) result ^= Name.GetHashCode();
                result ^= Number.GetHashCode();
                return result;
            }

            public override bool Equals(object obj)
            {
                var other = obj as VersionedType;
                if (other == null) return false;
                if (this.Name != other.Name) return false;
                if (this.Number != other.Number) return false;
                return true;
            }

            public VersionedType Clone()
            {
                return new VersionedType(this);
            }
        }

        [AsyncTestMethod]
        [Tag("notpassing")]
        public async Task CreateFilterTestWithMultipleRequests()
        {
            await CreateFilterTestWithMultipleRequests(true);
            await CreateFilterTestWithMultipleRequests(false);
        }

        [AsyncTestMethod]
        public async Task ValidateUserAgent()
        {
            await CreateUserAgentValidationTest();
        }

        [AsyncTestMethod]
        public async Task ParameterPassingTests()
        {
            await CreateParameterPassingTest(true);
            await CreateParameterPassingTest(false);
        }

        [AsyncTestMethod]
        public async Task OptimisticConcurrency_ClientSide()
        {

            await CreateOptimisticConcurrencyTest("Conflicts (client side) - client wins", (clientItem, serverItem) =>
            {
                var mergeResult = clientItem.Clone();
                mergeResult.Version = serverItem.Version;
                return mergeResult;
            });
            await CreateOptimisticConcurrencyTest("Conflicts (client side) - server wins", (clientItem, serverItem) =>
            {
                return serverItem;
            });
            await CreateOptimisticConcurrencyTest("Conflicts (client side) - Name from client, Number from server", (clientItem, serverItem) =>
            {
                var mergeResult = serverItem.Clone();
                mergeResult.Name = clientItem.Name;
                return mergeResult;
            });
        }

        [AsyncTestMethod]
        public async Task OptimisticConcurrency_ServerSide_ClientWins()
        {
            await CreateOptimisticConcurrencyWithServerConflictsTest("Conflicts (server side) - client wins", true);
        }

        [AsyncTestMethod]
        public async Task OptimisticConcurrency_ServerSide_ServerWins()
        {
            await CreateOptimisticConcurrencyWithServerConflictsTest("Conflicts (server side) - server wins", false);
        }

        [AsyncTestMethod]
        public async Task SystemPropertiesTests()
        {
            await CreateSystemPropertiesTest(true);
            await CreateSystemPropertiesTest(false);
        }



        private async Task CreateSystemPropertiesTest(bool useTypedTable)
        {
            Log("### System properties in " + (useTypedTable ? "" : "un") + "typed tables");

            var client = this.GetClient();
            var typedTable = client.GetTable<VersionedType>();
            var untypedTable = client.GetTable("RoundTripTable");

            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using seed: {0}", seed);
            Random rndGen = new Random(seed);
            VersionedType item = null;
            JObject untypedItem = null;
            DateTime createdAt, updatedAt;
            string id;
            if (useTypedTable)
            {
                item = new VersionedType(rndGen);
                await typedTable.InsertAsync(item);
                Log("Inserted: {0}", item);
                id = item.Id;
                createdAt = item.CreatedAt;
                updatedAt = item.UpdatedAt;
            }
            else
            {
                untypedItem = new JObject();
                untypedItem.Add("name", "unused");
                untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                Log("Inserted: {0}", untypedItem);
                id = (string)untypedItem["id"];
                createdAt = untypedItem["createdAt"].ToObject<DateTime>();
                updatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            Log("Now adding a new item");
            DateTime otherCreatedAt, otherUpdatedAt;
            string otherId;
            if (useTypedTable)
            {
                item = new VersionedType(rndGen);
                await typedTable.InsertAsync(item);
                Log("Inserted: {0}", item);
                otherId = item.Id;
                otherCreatedAt = item.CreatedAt;
                otherUpdatedAt = item.UpdatedAt;
            }
            else
            {
                untypedItem = new JObject();
                untypedItem.Add("name", "unused");
                untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                Log("Inserted: {0}", untypedItem);
                otherId = (string)untypedItem["id"];
                otherCreatedAt = untypedItem["createdAt"].ToObject<DateTime>();
                otherUpdatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            if (createdAt >= otherCreatedAt)
            {
                Assert.Fail("Error, first createdAt value is not smaller than second one");
            }

            if (updatedAt >= otherUpdatedAt)
            {
                Assert.Fail("Error, first updatedAt value is not smaller than second one");
            }

            createdAt = otherCreatedAt;
            updatedAt = otherUpdatedAt;

            Log("Now updating the item");
            if (useTypedTable)
            {
                item = new VersionedType(rndGen) { Id = otherId };
                await typedTable.UpdateAsync(item);
                Log("Updated: {0}", item);
                otherUpdatedAt = item.UpdatedAt;
                otherCreatedAt = item.CreatedAt;
            }
            else
            {
                untypedItem = new JObject(new JProperty("id", otherId), new JProperty("name", "other name"));
                untypedItem = (JObject)(await untypedTable.UpdateAsync(untypedItem));
                Log("Updated: {0}", untypedItem);
                otherCreatedAt = untypedItem["createdAt"].ToObject<DateTime>();
                otherUpdatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            if (createdAt != otherCreatedAt)
            {
                Assert.Fail("Error, update changed the value of the createdAt property");
            }

            if (otherUpdatedAt <= updatedAt)
            {
                Assert.Fail("Error, update did not change the updatedAt property to a later value");
            }

            Log("Cleanup: deleting items");
            await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
            await untypedTable.DeleteAsync(new JObject(new JProperty("id", otherId)));
        }

        private async Task CreateOptimisticConcurrencyTest(string testName, Func<VersionedType, VersionedType, VersionedType> mergingPolicy)
        {
            Log("### " + testName);
            var client = this.GetClient();
            var table = client.GetTable<VersionedType>();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using seed: {0}", seed);
            Random rndGen = new Random(seed);
            var item = new VersionedType(rndGen);
            await table.InsertAsync(item);
            Log("[client 1] Inserted item: {0}", item);

            var client2 = new MobileServiceClient(client.MobileAppUri);
            var table2 = client.GetTable<VersionedType>();
            var item2 = await table2.LookupAsync(item.Id);
            Log("[client 2] Retrieved the item");
            item2.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
            item2.Number = rndGen.Next(100000);
            Log("[client 2] Updated the item, will update on the server now");
            await table2.UpdateAsync(item2);
            Log("[client 2] Item has been updated: {0}", item2);

            Log("[client 1] Will try to update; should fail");
            MobileServicePreconditionFailedException<VersionedType> ex = null;
            try
            {
                item.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
                await table.UpdateAsync(item);
                Assert.Fail(string.Format("[client 1] Error, the update succeeded, but it should have failed. Item = {0}", item));
            }
            catch (MobileServicePreconditionFailedException<VersionedType> e)
            {
                Log("[client 1] Received expected exception; server item = {0}", e.Item);
                ex = e;
            }

            var serverItem = ex.Item;
            if (serverItem.Version != item2.Version)
            {
                Assert.Fail("[client 1] Error, server item's version is not the same as the second item version");
            }

            var cachedMergedItem = mergingPolicy(item, serverItem);
            var mergedItem = mergingPolicy(item, serverItem);
            Log("[client 1] Merged item: {0}", mergedItem);
            Log("[client 1] Trying to update it again, should succeed this time");

            await table.UpdateAsync(mergedItem);
            Log("[client 1] Updated the item: {0}", mergedItem);

            if (!cachedMergedItem.Equals(mergedItem))
            {
                Assert.Fail("[client 1] Error, the server version of the merged item doesn't match the client one");
            }

            Log("[client 2] Refreshing the item");
            await table2.RefreshAsync(item2);
            Log("[client 2] Refreshed the item: {0}", item2);

            if (!item2.Equals(mergedItem))
            {
                Assert.Fail("[client] Error, item is different than the item from the client 1");
            }
        }

        private async Task CreateOptimisticConcurrencyWithServerConflictsTest(string testName, bool clientWins)
        {
            Log("### " + testName);

            var client = this.GetClient();
            var table = client.GetTable<VersionedType>();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Log("Using seed: {0}", seed);
            Random rndGen = new Random(seed);
            var item = new VersionedType(rndGen);
            await table.InsertAsync(item);
            Log("[client 1] Inserted item: {0}", item);

            var client2 = new MobileServiceClient(client.MobileAppUri);
            var table2 = client.GetTable<VersionedType>();
            var item2 = await table2.LookupAsync(item.Id);
            Log("[client 2] Retrieved the item");
            item2.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
            item2.Number = rndGen.Next(100000);
            Log("[client 2] Updated the item, will update on the server now");
            await table2.UpdateAsync(item2);
            Log("[client 2] Item has been updated: {0}", item2);

            Log("[client 1] Will try to update - using policy that data on {0} wins", clientWins ? "client" : "server");
            string oldName = item2.Name;
            string newName = Utilities.CreateSimpleRandomString(rndGen, 20);
            item.Name = newName;
            await table.UpdateAsync(item, new Dictionary<string, string> { { "conflictPolicy", clientWins ? "clientWins" : "serverWins" } });
            Log("[client 1] Updated the item: {0}", item);

            Log("[client 2] Now refreshing the second item");
            await table2.RefreshAsync(item2);
            Log("[client 2] Refreshed: {0}", item2);
            if (clientWins)
            {
                // The name should be the new one
                if (item.Name != newName || item2.Name != newName)
                {
                    Assert.Fail("Error, name wasn't updated in a 'client wins' policy");
                }
            }
            else
            {
                // The name should have remained the old one
                if (item.Name != oldName || item2.Name != oldName)
                {
                    Assert.Fail("Error, name was updated in a 'server wins' policy");
                }
            }

            Log("Table operations behaved as expected. Cleaning up...");
            await table.DeleteAsync(item);
            Log("...done");
        }

        private async Task CreateParameterPassingTest(bool useTypedTable)
        {
            Log("### Parameter passing test - " + (useTypedTable ? "typed" : "untyped") + " tables");

            var client = this.GetClient();
            var typed = client.GetTable<ParamsTestTableItem>();
            var untyped = client.GetTable("ParamsTestTable");
            var dict = new Dictionary<string, string>
                {
                    { "item", "simple" },
                    { "empty", "" },
                    { "spaces", "with spaces" },
                    { "specialChars", "`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?" },
                    { "latin", "ãéìôü ÇñÑ" },
                    { "arabic", "الكتاب على الطاولة" },
                    { "chinese", "这本书在桌子上" },
                    { "japanese", "本は机の上に" },
                    { "hebrew", "הספר הוא על השולחן" },
                    { "name+with special&chars", "should just work" }
                };

            var expectedParameters = new JObject();
            foreach (var key in dict.Keys)
            {
                expectedParameters.Add(key, dict[key]);
            }

            bool testPassed = true;

            ParamsTestTableItem typedItem = new ParamsTestTableItem();
            var untypedItem = new JObject();
            JObject actualParameters;

            dict["operation"] = "insert";
            expectedParameters.Add("operation", "insert");
            if (useTypedTable)
            {
                await typed.InsertAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
            }
            else
            {
                var inserted = await untyped.InsertAsync(untypedItem, dict);
                untypedItem = inserted as JObject;
                actualParameters = JObject.Parse(untypedItem["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("insert", expectedParameters, actualParameters);

            dict["operation"] = "update";
            expectedParameters["operation"] = "update";
            if (useTypedTable)
            {
                await typed.UpdateAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
            }
            else
            {
                var updated = await untyped.UpdateAsync(untypedItem, dict);
                actualParameters = JObject.Parse(updated["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("update", expectedParameters, actualParameters);

            dict["operation"] = "lookup";
            expectedParameters["operation"] = "lookup";
            if (useTypedTable)
            {
                var temp = await typed.LookupAsync(1, dict);
                actualParameters = JObject.Parse(temp.parameters);
            }
            else
            {
                var temp = await untyped.LookupAsync(1, dict);
                actualParameters = JObject.Parse(temp["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("lookup", expectedParameters, actualParameters);

            dict["operation"] = "read";
            expectedParameters["operation"] = "read";
            if (useTypedTable)
            {
                var temp = await typed.Where(t => t.Id >= 1).WithParameters(dict).ToListAsync();
                actualParameters = JObject.Parse(temp[0].parameters);
            }
            else
            {
                var temp = await untyped.ReadAsync("$filter=id ge 1", dict);
                actualParameters = JObject.Parse(temp[0]["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("read", expectedParameters, actualParameters);

            if (useTypedTable)
            {
                // Refresh operation only exists for typed tables
                dict["operation"] = "read";
                expectedParameters["operation"] = "read";
                typedItem.Id = 1;
                typedItem.parameters = "";
                await typed.RefreshAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
                testPassed = testPassed && ValidateParameters("refresh", expectedParameters, actualParameters);
            }

            // Delete operation doesn't populate the object with the response, so we'll use a filter to capture that
            var handler = new HandlerToCaptureHttpTraffic();
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);
            typed = filteredClient.GetTable<ParamsTestTableItem>();
            untyped = filteredClient.GetTable("ParamsTestTable");

            dict["operation"] = "delete";
            expectedParameters["operation"] = "delete";
            if (useTypedTable)
            {
                await typed.DeleteAsync(typedItem, dict);
            }
            else
            {
                await untyped.DeleteAsync(untypedItem, dict);
            }

            JObject response = JObject.Parse(handler.ResponseBody);
            actualParameters = JObject.Parse(response["parameters"].Value<string>());

            testPassed = testPassed && ValidateParameters("delete", expectedParameters, actualParameters);

            if (!testPassed)
            {
                Assert.Fail("");
            }
        }

        private bool ValidateParameters(string operation, JObject expected, JObject actual)
        {
            Log(string.Format("Called {0}, now validating parameters", operation));
            List<string> errors = new List<string>();
            if (!Utilities.CompareJson(expected, actual, errors))
            {
                foreach (var error in errors)
                {
                    Log(error);
                }

                Log("Parameters passing for the {0} operation failed", operation);
                Log("Expected: {0}", expected);
                Log("Actual: {0}", actual);
                return false;
            }
            else
            {
                Log("Parameters passing for the {0} operation succeeded", operation);
                return true;
            }
        }

        private async Task CreateUserAgentValidationTest()
        {
            Log("Validation User-Agent header");

            var handler = new HandlerToCaptureHttpTraffic();
            MobileServiceClient client = new MobileServiceClient(
                this.GetTestSetting("MobileServiceRuntimeUrl"),
                handler);
            var table = client.GetTable<RoundTripTableItem>();
            var item = new RoundTripTableItem { Name = "hello" };
            await table.InsertAsync(item);
            Action<string> dumpAndValidateHeaders = delegate(string operation)
            {
                Log("Headers for {0}:", operation);
                Log("  Request:");
                foreach (var header in handler.RequestHeaders.Keys)
                {
                    Log("    {0}: {1}", header, handler.RequestHeaders[header]);
                }

                Log("  Response:");
                foreach (var header in handler.ResponseHeaders.Keys)
                {
                    Log("    {0}: {1}", header, handler.ResponseHeaders[header]);
                }

                string userAgent;
                if (!handler.RequestHeaders.TryGetValue("User-Agent", out userAgent))
                {
                    Log("No user-agent header in the request");
                    throw new InvalidOperationException("This will fail the test");
                }
                else
                {
                    Regex expected = new Regex(@"^ZUMO\/\d.\d");
                    if (expected.IsMatch(userAgent))
                    {
                        Log("User-Agent validated correclty");
                    }
                    else
                    {
                        Log("User-Agent didn't validate properly.");
                        throw new InvalidOperationException("This will fail the test");
                    }
                }
            };

            dumpAndValidateHeaders("Insert");

            item.Number = 123;
            await table.UpdateAsync(item);
            dumpAndValidateHeaders("Update");

            var item2 = await table.LookupAsync(item.Id);
            dumpAndValidateHeaders("Read");

            await table.DeleteAsync(item);
            dumpAndValidateHeaders("Delete");
        }

        private async Task CreateFilterTestWithMultipleRequests(bool typed)
        {
            Log(string.Format(CultureInfo.InvariantCulture, "### Filter which maps one requests to many - {0} client", typed ? "typed" : "untyped"));

            var client = this.GetClient();
            int numberOfRequests = new Random().Next(2, 5);
            var handler = new HandlerWithMultipleRequests(this, numberOfRequests);
            Log("Created a filter which will replay the request {0} times", numberOfRequests);
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);

            var typedTable = filteredClient.GetTable<RoundTripTableItem>();
            var untypedTable = filteredClient.GetTable("RoundTripTable");
            var uniqueId = Guid.NewGuid().ToString("N");
            if (typed)
            {
                try
                {
                    var item = new RoundTripTableItem { Name = uniqueId };
                    await typedTable.InsertAsync(item);
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    Log($"Exception while inserting: {ex}");
                    throw;
                }

            }
            else
            {
                var item = new JObject(new JProperty("name", uniqueId));
                await untypedTable.InsertAsync(item);
            }

            if (handler.TestFailed)
            {
                Assert.Fail("Filter reported a test failure. Aborting.");
            }

            Log("Inserted the data; now retrieving it to see how many items we have inserted.");
            handler.NumberOfRequests = 1; // no need to send it multiple times anymore

            var items = await untypedTable.ReadAsync("$select=name,id&$filter=name eq '" + uniqueId + "'");
            var array = (JArray)items;
            bool passed;
            if (array.Count == numberOfRequests)
            {
                Log("Filter inserted correct number of items.");
                passed = true;
            }
            else
            {
                Log("Error, filtered client should have inserted {0} items, but there are {1}", numberOfRequests, array.Count);
                passed = false;
            }

            // Cleanup
            foreach (var item in array)
            {
                await untypedTable.DeleteAsync(item as JObject);
            }

            Log("Cleanup: removed added items.");
            if (!passed)
            {
                Assert.Fail("");
            }
        }

        class HandlerWhichThrows : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        class HandlerToBypassService : DelegatingHandler
        {
            HttpStatusCode statusCode;
            string contentType;
            string content;

            public HandlerToBypassService(int statusCode, string contentType, string content)
            {
                this.statusCode = (HttpStatusCode)statusCode;
                this.contentType = contentType;
                this.content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                HttpResponseMessage result = new HttpResponseMessage(this.statusCode);
                result.Content = new StringContent(this.content, Encoding.UTF8, this.contentType);
                tcs.SetResult(result);
                return tcs.Task;
            }
        }

        class HandlerToCaptureHttpTraffic : DelegatingHandler
        {
            public Dictionary<string, string> RequestHeaders { get; private set; }
            public Dictionary<string, string> ResponseHeaders { get; private set; }
            public string ResponseBody { get; set; }

            public HandlerToCaptureHttpTraffic()
            {
                this.RequestHeaders = new Dictionary<string, string>();
                this.ResponseHeaders = new Dictionary<string, string>();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestHeaders.Clear();
                foreach (var header in request.Headers)
                {
                    this.RequestHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
                    {
                        string userAgent = this.RequestHeaders[header.Key];
                        userAgent.TrimEnd(')');
                        int equalsIndex = userAgent.LastIndexOf('=');
                        if (equalsIndex >= 0)
                        {
                            //var clientVersion = userAgent.Substring(equalsIndex + 1);
                            //ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.ClientVersionKeyName] = clientVersion;
                        }
                    }
                }

                var response = await base.SendAsync(request, cancellationToken);
                this.ResponseHeaders.Clear();
                foreach (var header in response.Headers)
                {
                    this.ResponseHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("x-zumo-version", StringComparison.OrdinalIgnoreCase))
                    {
                        //ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.RuntimeVersionKeyName] = this.ResponseHeaders[header.Key];
                    }
                }

                this.ResponseBody = await response.Content.ReadAsStringAsync();
                return response;
            }
        }

        class HandlerWithMultipleRequests : DelegatingHandler
        {
            private FunctionalTestBase Parent { get; set; }
            public bool TestFailed { get; private set; }
            public int NumberOfRequests { get; set; }

            public HandlerWithMultipleRequests(FunctionalTestBase parent, int numberOfRequests)
            {
                this.Parent = parent;
                this.NumberOfRequests = numberOfRequests;
                this.TestFailed = false;

                if (numberOfRequests < 1)
                {
                    throw new ArgumentOutOfRangeException("numberOfRequests", "Number of requests must be at least 1.");
                }
            }

            private static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
            {
                HttpRequestMessage result = new HttpRequestMessage(request.Method, request.RequestUri);
                if (request.Content != null)
                {
                    string content = await request.Content.ReadAsStringAsync();
                    string mediaType = request.Content.Headers.ContentType.MediaType;
                    result.Content = new StringContent(content, Encoding.UTF8, mediaType);
                }

                foreach (var header in request.Headers)
                {
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Headers.Add(header.Key, header.Value);
                    }
                }

                return result;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage response = null;
                try
                {
                    for (int i = 0; i < this.NumberOfRequests; i++)
                    {
                        HttpRequestMessage clonedRequest = await CloneRequest(request);
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                        if (i < this.NumberOfRequests - 1)
                        {
                            response.Dispose();
                            response = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Parent.Log("Exception while calling continuation: " + ex.ToString());
                    this.TestFailed = true;
                    throw;
                }

                return response;
            }
        }
    }
}
