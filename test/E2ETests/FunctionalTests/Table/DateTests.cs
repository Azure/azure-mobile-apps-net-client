// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataTable("dates")]
    public class Dates
    {
        public Dates()
        {
            this.Date = DateTime.UtcNow;
            this.DateOffset = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public DateTimeOffset DateOffset { get; set; }
    }


    [Tag("Date")]
    public class DateTests : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task InsertAndQuery()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTime date = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Log("Start: {0}", date);

            Log("Inserting instance");
            Dates instance = new Dates { Date = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.Date);

            Log("Querying for instance");
            List<Dates> items = await table.Where(i => i.Date == date).Where(i => i.Id == instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].Date);

            Log("Finish: {0}", items[0].Date);
        }

        [AsyncTestMethod]
        public async Task InsertAndQueryOffset()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTimeOffset date = new DateTimeOffset(
                new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Utc).ToLocalTime());
            Log("Start: {0}", date);

            Log("Inserting instance");
            Dates instance = new Dates { DateOffset = date };
            await table.InsertAsync(instance);
            Assert.AreEqual(date, instance.DateOffset);

            Log("Querying for instance");
            List<Dates> items = await table.Where(i => i.DateOffset == date).Where(i => i.Id == instance.Id).ToListAsync();
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(date, items[0].DateOffset);

            Log("Finish: {0}", items[0].DateOffset);
        }

        [AsyncTestMethod]
        public async Task DateKinds()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Dates instance = new Dates { Date = original };

            Log("Start Kind: {0}", instance.Date.Kind);
            await table.InsertAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
            Assert.AreEqual(original, instance.Date);

            Log("Change to UTC");
            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Utc);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);

            Log("Change to Local");
            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Local);
            await table.UpdateAsync(instance);
            Assert.AreEqual(DateTimeKind.Local, instance.Date.Kind);
        }

        [Tag("Date")]
        [AsyncTestMethod]
        public async Task ChangeCulture()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            CultureInfo threadCulture = CultureInfo.DefaultThreadCurrentCulture;
            CultureInfo threadUICulture = CultureInfo.DefaultThreadCurrentUICulture;

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Dates instance = new Dates { Date = original };
            await table.InsertAsync(instance);

            Log("Change culture to ar-EG");
            CultureInfo arabic = new CultureInfo("ar-EG");
            CultureInfo.DefaultThreadCurrentCulture = arabic;
            CultureInfo.DefaultThreadCurrentUICulture = arabic;
            Dates arInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, arInstance.Date);

            Log("Change culture to zh-CN");
            CultureInfo chinese = new CultureInfo("zh-CN");
            CultureInfo.DefaultThreadCurrentCulture = chinese;
            CultureInfo.DefaultThreadCurrentUICulture = chinese;
            Dates zhInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, zhInstance.Date);

            Log("Change culture to ru-RU");
            CultureInfo russian = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = russian;
            CultureInfo.DefaultThreadCurrentUICulture = russian;
            Dates ruInstance = await table.LookupAsync(instance.Id);
            Assert.AreEqual(original, ruInstance.Date);

            CultureInfo.DefaultThreadCurrentCulture = threadCulture;
            CultureInfo.DefaultThreadCurrentUICulture = threadUICulture;
        }
    }
}
