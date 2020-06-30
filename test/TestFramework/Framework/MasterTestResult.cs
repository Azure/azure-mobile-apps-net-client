// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    public class MasterTestResult
    {
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("backend")]
        public string Backend { get; set; }

        [JsonProperty("outcome")]
        public string Outcome { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("passed")]
        public int Passed { get; set; }

        [JsonProperty("failed")]
        public int Failed { get; set; }

        [JsonProperty("skipped")]
        public int Skipped { get; set; }

        [JsonProperty("start_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty("reference_url")]
        public string ReferenceUrl { get; set; }
    }
}
