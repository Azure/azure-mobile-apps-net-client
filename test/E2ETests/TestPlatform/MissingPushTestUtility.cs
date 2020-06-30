// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MissingPushTestUtility : IPushTestUtility
    {
        public string GetPushHandle()
        {
            throw new NotImplementedException();
        }
    }
}
