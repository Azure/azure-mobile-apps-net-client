// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataTable("types")]
    public class DateExample
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "DateExampleDate")]
        public DateTime Date { get; set; }
    }

    [DataTable("types")]
    public class DateOffsetExample
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "DateOffsetExampleDate")]
        public DateTimeOffset Date { get; set; }
    }
}
