// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace MobileTest.Framework
{
    /// <summary>
    /// Defines tte test result of an individual test
    /// </summary>
    public class TestResult
    {
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("outcome")]
        public string Outcome { get; set; }

        [JsonProperty("start_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty("reference_url")]
        public string ReferenceUrl { get; set; }
    }
}
