// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test.Functional
{
    public abstract class FunctionalTestBase
    {
        public MobileServiceClient GetClient()
        {
            var runtimeUrl = Environment.GetEnvironmentVariable("runtimeUrl");
            if (runtimeUrl == null)
            {
                throw new NotSupportedException("Runtime URL is not specified - Blogging Tests will be skipped");
            }
            return new MobileServiceClient(runtimeUrl);
        }
    }
}
