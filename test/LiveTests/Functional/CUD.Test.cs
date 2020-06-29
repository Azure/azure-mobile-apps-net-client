// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using MobileTest.Framework;
using MobileTest.LiveTests.Base;
using MobileTest.LiveTests.TestData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MobileTest.LiveTests.Functional
{
    [Tag("CUD")]
    public class CUDTests : FunctionalTestBase
    {
        enum DeleteTestType { ValidDelete, NonExistingId, NoIdField }

        static Lazy<Random> s_Random = new Lazy<Random>(() =>
        {
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            return new Random(seed);
        });

        [AsyncTestMethod]
        private async Task CUD_TypedStringId()
        {
            Random rndGen = s_Random.Value;

            await CreateTypedUpdateTest("[string id] Update typed item",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen));
            await CreateTypedUpdateTest(
                "[string id] Update typed item, setting values to null",
                new RoundTripTableItem(rndGen),
                new RoundTripTableItem(rndGen) { Name = null, Bool = null, Date = null });
            await CreateTypedUpdateTest(
                "[string id] Update typed item, setting values to 0",
                new RoundTripTableItem(rndGen),
                new RoundTripTableItem(rndGen) { Integer = 0, Number = 0.0 });

            await CreateTypedUpdateTest<RoundTripTableItem, MobileServiceInvalidOperationException>("[string id] (Neg) Update typed item, non-existing item id",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = "does not exist" }, false);
            await CreateTypedUpdateTest<RoundTripTableItem, ArgumentException>("[string id] (Neg) Update typed item, id = null",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = null }, false);
        }

        [AsyncTestMethod]
        private async Task CUD_TypedIntId()
        {
            Random rndGen = s_Random.Value;

            await CreateTypedUpdateTest("[int id] Update typed item", new IntIdRoundTripTableItem(rndGen), new IntIdRoundTripTableItem(rndGen));
            await CreateTypedUpdateTest("[int id] Update typed item, setting values to null",
                new IntIdRoundTripTableItem(rndGen),
                new IntIdRoundTripTableItem(rndGen) { Name = null, Bool = null, Date = null });
            await CreateTypedUpdateTest<IntIdRoundTripTableItem, MobileServiceInvalidOperationException>("[int id] (Neg) Update typed item, non-existing item id",
                new IntIdRoundTripTableItem(rndGen), new IntIdRoundTripTableItem(rndGen) { Id = 1000000000 }, false);
            await CreateTypedUpdateTest<IntIdRoundTripTableItem, ArgumentException>("[int id] (Neg) Update typed item, id = 0",
                new IntIdRoundTripTableItem(rndGen), new IntIdRoundTripTableItem(rndGen) { Id = 0 }, false);
        }

        [AsyncTestMethod]
        private async Task CUD_UntypedStringId()
        {
            Random rndGen = s_Random.Value;

            string toInsertJsonString = JsonConvert.SerializeObject(new RoundTripTableItem(rndGen) { Id = null });
            string toUpdateJsonString = JsonConvert.SerializeObject(new RoundTripTableItem(rndGen) { Id = null });
            await CreateUntypedUpdateTest("[string id] Update untyped item", toInsertJsonString, toUpdateJsonString, true);

            JToken nullValue = JValue.Parse("null");
            JObject toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["name"] = nullValue;
            toUpdate["date1"] = nullValue;
            toUpdate["bool"] = nullValue;
            toUpdate["number"] = nullValue;
            await CreateUntypedUpdateTest("[string id] Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString(), true);

            string idName = GetSerializedId<RoundTripTableItem>();

            toUpdate[idName] = Guid.NewGuid().ToString();
            await CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("(Neg) [string id] Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false, true);

            toUpdate[idName] = nullValue;
            await CreateUntypedUpdateTest<InvalidOperationException>("[string id] (Neg) Update typed item, id = null",
                toInsertJsonString, toUpdateJsonString, false, true);

            // Delete tests
            await CreateDeleteTest<RoundTripTableItem>("[string id] Delete typed item", true, DeleteTestType.ValidDelete);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId);
            await CreateDeleteTest<RoundTripTableItem>("[string id] Delete untyped item", false, DeleteTestType.ValidDelete);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField);
        }

        [AsyncTestMethod]
        private async Task CUD_UntypedIntId()
        {
            Random rndGen = s_Random.Value;

            string toInsertJsonString = @"{
                ""name"":""hello"",
                ""date1"":""2012-12-13T09:23:12.000Z"",
                ""bool"":true,
                ""integer"":-1234,
                ""number"":123.45
            }";

            string toUpdateJsonString = @"{
                ""name"":""world"",
                ""date1"":""1999-05-23T19:15:54.000Z"",
                ""bool"":false,
                ""integer"":9999,
                ""number"":888.88
            }";

            await CreateUntypedUpdateTest("[int id] Update untyped item", toInsertJsonString, toUpdateJsonString);

            JToken nullValue = JValue.Parse("null");
            JObject toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["name"] = nullValue;
            toUpdate["bool"] = nullValue;
            toUpdate["integer"] = nullValue;
            await CreateUntypedUpdateTest("[int id] Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString());

            string idName = GetSerializedId<IntIdRoundTripTableItem>();

            toUpdate[idName] = 1000000000;
            await CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("[int id] (Neg) Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false);

            toUpdate[idName] = 0;
            await CreateUntypedUpdateTest<ArgumentException>("[int id] (Neg) Update typed item, id = 0",
                toInsertJsonString, toUpdateJsonString, false);

            // Delete tests
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] Delete typed item", true, DeleteTestType.ValidDelete);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] Delete untyped item", false, DeleteTestType.ValidDelete);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField);
        }


        private Task CreateTypedUpdateTest<TRoundTripType>(
                    string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate) where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            return CreateTypedUpdateTest<TRoundTripType, ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate);
        }

        private async Task CreateTypedUpdateTest<TRoundTripType, TExpectedException>(
            string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate, bool setUpdatedId = true)
            where TExpectedException : Exception
            where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            Log("### Executing {0}.", testName);

            var client = GetClient();

            var table = client.GetTable<TRoundTripType>();
            var toInsert = itemToInsert.Clone();
            var toUpdate = itemToUpdate.Clone();
            try
            {
                await table.InsertAsync(toInsert);
                Log("Inserted item with id {0}", toInsert.Id);

                if (setUpdatedId)
                {
                    toUpdate.Id = toInsert.Id;
                }

                var expectedItem = toUpdate.Clone();

                await table.UpdateAsync(toUpdate);
                Log("Updated item; now retrieving it to compare with the expected value");

                var retrievedItem = await table.LookupAsync(toInsert.Id);
                Log("Retrieved item");

                Assert.AreEqual(expectedItem, retrievedItem);

                // cleanup
                await table.DeleteAsync(retrievedItem);

                if (typeof(TExpectedException) != typeof(ExceptionTypeWhichWillNeverBeThrown))
                {
                    Assert.Fail("Error, test should have failed with " + typeof(TExpectedException).FullName + ", but succeeded.");
                }
            }
            catch (TExpectedException ex)
            {
                Log("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
            }
        }

        private Task CreateUntypedUpdateTest(
            string testName, string itemToInsert, string itemToUpdate, bool useStringIdTable = false)
        {
            return CreateUntypedUpdateTest<ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate, true, useStringIdTable);
        }

        private async Task CreateUntypedUpdateTest<TExpectedException>(
            string testName, string itemToInsertJson, string itemToUpdateJson, bool setUpdatedId = true, bool useStringIdTable = false)
            where TExpectedException : Exception
        {
            Log("### Executing {0}.", testName);

            var itemToInsert = JObject.Parse(itemToInsertJson);
            var itemToUpdate = JObject.Parse(itemToUpdateJson);
            CamelCaseProps(itemToUpdate);

            var client = GetClient();
            var table = client.GetTable(useStringIdTable ? "RoundTripTable" : "IntIdRoundTripTable");
            try
            {
                var inserted = await table.InsertAsync(itemToInsert);
                object id = useStringIdTable ?
                    (object)(string)inserted["id"] :
                    (object)(int)inserted["id"];

                Log("Inserted item with id {0}", id);

                if (setUpdatedId)
                {
                    itemToUpdate["id"] = new JValue(id);
                }

                var expectedItem = JObject.Parse(itemToUpdate.ToString());

                var updated = await table.UpdateAsync(itemToUpdate);
                Log("Updated item; now retrieving it to compare with the expected value");

                var retrievedItem = await table.LookupAsync(id);
                Log("Retrieved item");

                List<string> errors = new List<string>();
                if (!Utilities.CompareJson(expectedItem, retrievedItem, errors))
                {
                    foreach (var error in errors)
                    {
                        Log(error);
                    }

                    Assert.Fail("Error, retrieved item is different than the expected value. Expected: " + expectedItem + "; actual:" + retrievedItem);
                    return;
                }

                // cleanup
                await table.DeleteAsync(new JObject(new JProperty("id", id)));

                if (typeof(TExpectedException) != typeof(ExceptionTypeWhichWillNeverBeThrown))
                {
                    Assert.Fail("Error, test should have failed with " + typeof(TExpectedException).FullName + " but succeeded.");
                }
            }
            catch (TExpectedException ex)
            {
                Log("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
            }
        }

        private async Task CreateDeleteTest<TItemType>(string testName, bool useTypedTable, DeleteTestType testType) where TItemType : ICloneableItem<TItemType>
        {
            if (useTypedTable && testType == DeleteTestType.NoIdField)
            {
                throw new ArgumentException("Cannot send a delete request without an id field on a typed table.");
            }

            var client = GetClient();
            var typedTable = client.GetTable<TItemType>();
            var useStringIdTable = typeof(TItemType) == typeof(RoundTripTableItem);
            var untypedTable = client.GetTable(useStringIdTable ? "RoundTripTable" : "IntIdRoundTripTable");
            TItemType itemToInsert;
            if (useStringIdTable)
            {
                itemToInsert = (TItemType)(object)new RoundTripTableItem { Name = "will be deleted", Number = 123 };
            }
            else
            {
                itemToInsert = (TItemType)(object)new IntIdRoundTripTableItem { Name = "will be deleted", Number = 123 };
            }

            await typedTable.InsertAsync(itemToInsert);
            Log("Inserted item to be deleted");
            object id = itemToInsert.Id;
            switch (testType)
            {
                case DeleteTestType.ValidDelete:
                    if (useTypedTable)
                    {
                        await typedTable.DeleteAsync(itemToInsert);
                    }
                    else
                    {
                        await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
                    }

                    Log("Delete succeeded; verifying that object isn't in the service anymore.");
                    try
                    {
                        var response = await untypedTable.LookupAsync(id);
                        Assert.Fail("Error, delete succeeded, but item was returned by the service: " + response);
                    }
                    catch (MobileServiceInvalidOperationException msioe)
                    {
                        Assert.IsTrue(Validate404Response(msioe));
                    }
                    return;

                case DeleteTestType.NonExistingId:
                    try
                    {
                        object nonExistingId = useStringIdTable ? (object)Guid.NewGuid().ToString() : (object)1000000000;
                        if (useTypedTable)
                        {
                            itemToInsert.Id = nonExistingId;
                            await typedTable.DeleteAsync(itemToInsert);
                        }
                        else
                        {
                            JObject jo = new JObject(new JProperty("id", nonExistingId));
                            await untypedTable.DeleteAsync(jo);
                        }
                        Assert.Fail("Error, deleting item with non-existing id should fail, but succeeded");
                    }
                    catch (MobileServiceInvalidOperationException msioe)
                    {
                        Assert.IsTrue(Validate404Response(msioe));
                    }
                    return;

                default:
                    try
                    {
                        JObject jo = new JObject(new JProperty("Name", "hello"));
                        await untypedTable.DeleteAsync(jo);

                        Assert.Fail("Error, deleting item without an id should fail, but succeeded");
                    }
                    catch (ArgumentException ex)
                    {
                        Log("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    }
                    return;
            }
        }

        private bool Validate404Response(MobileServiceInvalidOperationException msioe)
        {
            Log("Received expected exception - {0}: {1}", msioe.GetType().FullName, msioe.Message);
            var response = msioe.Response;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Log("And error code is the expected one.");
                return true;
            }
            else
            {
                Log("Received error code is not the expected one: {0} - {1}", response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }

        public static string GetSerializedId<T>()
        {
            var idName = typeof(T).GetTypeInfo()
                .DeclaredProperties
                .Where(p => p.Name.ToLowerInvariant() == "id")
                .Select(p =>
                {
                    var a = p.GetCustomAttribute<JsonPropertyAttribute>();
                    return a == null ? p.Name : a.PropertyName;
                })
                .First();
            return idName;
        }

        public static void CamelCaseProps(JObject itemToUpdate)
        {
            List<string> keys = new List<string>();
            foreach (var x in itemToUpdate)
            {
                keys.Add(x.Key);
            }

            foreach (var key in keys)
            {
                if (char.IsUpper(key[0]))
                {
                    StringBuilder camel = new StringBuilder(key);
                    camel[0] = Char.ToLowerInvariant(key[0]);
                    itemToUpdate[camel.ToString()] = itemToUpdate[key];
                    itemToUpdate.Remove(key);
                }
            }
        }
    }
}
