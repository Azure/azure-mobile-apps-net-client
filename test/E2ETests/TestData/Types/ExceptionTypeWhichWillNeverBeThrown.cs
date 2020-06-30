// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    // Used as the type parameter to positive tests
    internal class ExceptionTypeWhichWillNeverBeThrown : Exception
    {
        private ExceptionTypeWhichWillNeverBeThrown() { }

        public ExceptionTypeWhichWillNeverBeThrown(string message) : base(message)
        {
        }

        public ExceptionTypeWhichWillNeverBeThrown(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
