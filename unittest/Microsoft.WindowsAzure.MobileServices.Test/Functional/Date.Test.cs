// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    [TestClass]
    public class Date_Test : FunctionalTestBase
    {
        [TestMethod]
        public async Task DateUri()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable<DateExample> table = client.GetTable<DateExample>();

            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.SetResponseContent("[]");

            // Verify a full UTC date
            DateTime date = new DateTime(2009, 11, 21, 14, 22, 59, 860, DateTimeKind.Utc);
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            StringAssert.EndsWith(hijack.Request.RequestUri.ToString(), "$filter=(DateExampleDate eq datetime'2009-11-21T14%3A22%3A59.860Z')");

            // Local date is converted to UTC
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.SetResponseContent("[]");
            date = new DateTime(2009, 11, 21, 14, 22, 59, 860, DateTimeKind.Local);
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            StringAssert.EndsWith(hijack.Request.RequestUri.ToString(), "Z')");
        }

        [TestMethod]
        public async Task DateOffsetUri()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable<DateOffsetExample> table = client.GetTable<DateOffsetExample>();

            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.SetResponseContent("[]");

            var date = DateTimeOffset.Parse("2009-11-21T06:22:59.8600000-08:00");
            await table.Where(b => b.Date == date).ToEnumerableAsync();
            StringAssert.EndsWith(hijack.Request.RequestUri.ToString(), "$filter=(DateOffsetExampleDate eq datetimeoffset'2009-11-21T06%3A22%3A59.8600000-08%3A00')");
        }

        [TestCategory("live-tests")]
        [TestMethod, Ignore]
        public async Task InsertAndQuery()
        {
            IMobileServiceTable<DateExample> table = GetClient().GetTable<DateExample>();

            DateTime date = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            DateExample instance = new DateExample { Date = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.Date);

            List<DateExample> items = await table.Where(i => i.Date == date).Where(i => i.Id >= instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].Date);
        }

        [TestCategory("live-tests")]
        [TestMethod, Ignore]
        public async Task InsertAndQueryOffset()
        {
            IMobileServiceTable<DateOffsetExample> table = GetClient().GetTable<DateOffsetExample>();

            DateTimeOffset date = new DateTimeOffset(
                new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Utc).ToLocalTime());
            DateOffsetExample instance = new DateOffsetExample { Date = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.Date);

            List<DateOffsetExample> items = await table.Where(i => i.Date == date).Where(i => i.Id >= instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].Date);
        }

        [TestCategory("live-tests")]
        [TestMethod, Ignore]
        public async Task DateKinds()
        {
            IMobileServiceTable<DateExample> table = GetClient().GetTable<DateExample>();

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            DateExample instance = new DateExample { Date = original };

            await table.InsertAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
            Assert.AreEqual(original, instance.Date);

            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Utc);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);

            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Local);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
        }
    }
}
