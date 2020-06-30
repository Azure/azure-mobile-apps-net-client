using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    public class TestRun
    {
        /// <summary>
        /// Defines test metadata for a test run, including the test results for individual tests
        /// </summary>
        public TestRun()
        {
            this.Tags = new List<string>();
            this.testResults = new List<TestResult>();
            this.TestCount = 0;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time"), JsonConverter(typeof(DateTimeToWindowsFileTimeConverter))]
        public DateTime EndTime { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; private set; }

        [JsonProperty("test_count")]
        public int TestCount { get; set; }

        [JsonIgnore]
        private List<TestResult> testResults;

        public void AddTestResult(TestResult result)
        {
            this.testResults.Add(result);
            this.TestCount++;
        }

        [JsonIgnore]
        public IEnumerable<TestResult> TestResults { get { return this.testResults; } }
    }

    public class DateTimeToWindowsFileTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DateTime) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long fileTime = serializer.Deserialize<long>(reader);
            if (fileTime == 0)
            {
                return DateTime.MinValue;
            }
            else
            {
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dt = (DateTime)value;
            if (dt == DateTime.MinValue)
            {
                serializer.Serialize(writer, 0L);
            }
            else
            {
                serializer.Serialize(writer, dt.ToFileTimeUtc());
            }
        }
    }
}
