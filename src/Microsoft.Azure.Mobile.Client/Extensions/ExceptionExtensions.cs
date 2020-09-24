// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class ExceptionExtensions
    {
        public static bool IsNetworkError(this Exception ex)
        {
            return ex is HttpRequestException || ex is TimeoutException;
        }

        public static bool IsAuthenticationError(this Exception ex)
            => ex is MobileServiceInvalidOperationException ioEx 
                && ioEx.Response != null 
                && ioEx.Response.StatusCode == HttpStatusCode.Unauthorized;
    }
}
