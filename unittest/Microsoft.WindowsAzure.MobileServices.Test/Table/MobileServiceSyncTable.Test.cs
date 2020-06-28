// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Table
{
    [TestClass]
    public class MobileServiceSyncTable_Test
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateQueryId_Throws_OnInvalidId_1()
        {
            var queryId = "|myitems";
            MobileServiceSyncTable.ValidateQueryId(queryId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateQueryId_Throws_OnInvalidId_2()
        {
            var queryId = "s|myitems";
            MobileServiceSyncTable.ValidateQueryId(queryId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateQueryId_Throws_OnInvalidId_3()
        {
            var queryId = new String('n', 256);
            MobileServiceSyncTable.ValidateQueryId(queryId);
        }

        [TestMethod]
        public void ValidateQueryId_Succeeds_OnValidId()
        {
            var testCases = new[] { "myitems1", "myItems_yourItems1", "my-items123", "-myitems", "_myitems", "asdf@#$!/:^" };
            foreach (var queryId in testCases)
            {
                MobileServiceSyncTable.ValidateQueryId(queryId);
            }
        }

        [TestMethod]
        public async Task InsertAsync_GeneratesId_WhenNull()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            var item = new JObject();
            JObject inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.IsNotNull(inserted.Value<string>("id"), "Expected id member not found.");

            item = new JObject() { { "id", null } };
            inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.IsNotNull(inserted.Value<string>("id"), "Expected id member not found.");
        }
    }
}
