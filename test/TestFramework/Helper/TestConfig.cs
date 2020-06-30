// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    public class TestConfig
    {
        [JsonProperty("mobileAppUrl")]
        public string MobileServiceRuntimeUrl { get; set; }

        [JsonProperty("storageSasToken")]
        public string TestFrameworkStorageContainerSasToken { get; set; }

        [JsonProperty("storageUrl")]
        public string TestFrameworkStorageContainerUrl { get; set; }

        [JsonProperty("runTimeVersion")]
        public string RuntimeVersion { get; set; }

        [JsonProperty("tags")]
        public string TagExpression { get; set; }
    }
}
