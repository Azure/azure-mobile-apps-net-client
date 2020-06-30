// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MissingTestPlatform : ITestPlatform
    {
        public IPushTestUtility PushTestUtility
        {
            get { return new MissingPushTestUtility(); }
        }
    }
}
