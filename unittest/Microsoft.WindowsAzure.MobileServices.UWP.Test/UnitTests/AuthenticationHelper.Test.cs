// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("authentication")]
    [Tag("unit")]
    public class AuthenticationHelperTest:TestBase
    {
        [TestMethod]
        public void OnResponseReceived_FailureUriIsNull()
        {
            AuthenticationHelper helper = new AuthenticationHelper();
            Uri uri = null;
            helper.Error += (o, e) => { Assert.IsTrue(true); };
            helper.Completed += (o, e) => { Assert.IsTrue(false); };

            helper.OnResponseReceived(uri);
        }

        [TestMethod]
        public void OnResponseReceived_FailureUriInvalidScheme()
        {
            AuthenticationHelper helper = new AuthenticationHelper();
            string uriString = "zumoe2etestapp://invalid.easyauth.callback/#authorization_code=test_code";
            Uri uri = new Uri(uriString);
            helper.Error += (o, e) => { Assert.IsTrue(true); };
            helper.Completed += (o, e) => { Assert.IsTrue(false); };

            helper.OnResponseReceived(uri);
        }

        [TestMethod]
        public void OnResponseReceived_FailureUriNoAuthorizationCode()
        {
            AuthenticationHelper helper = new AuthenticationHelper();
            string uriString = "zumoe2etestapp://easyauth.callback/#";
            Uri uri = new Uri(uriString);
            helper.Error += (o, e) => { Assert.IsTrue(true); };
            helper.Completed += (o, e) => { Assert.IsTrue(false); };

            helper.OnResponseReceived(uri);
        }

        [TestMethod]
        public void OnResponseReceived_FailureUriParamWithNoValue()
        {
            AuthenticationHelper helper = new AuthenticationHelper();
            string uriString = "zumoe2etestapp://easyauth.callback/#authorization_code=test_code&invalid_param";
            Uri uri = new Uri(uriString);
            helper.Error += (o, e) => { Assert.IsTrue(true); };
            helper.Completed += (o, e) => { Assert.IsTrue(false); };

            helper.OnResponseReceived(uri);
        }

        [TestMethod]
        public void OnResponseReceived_Success()
        {
            AuthenticationHelper helper = new AuthenticationHelper();
            string authorizationCode = "test_code";
            string uriString = $"zumoe2etestapp://easyauth.callback/#authorization_code={authorizationCode}";
            Uri uri = new Uri(uriString);
            helper.Error += (o, e) => { Assert.IsTrue(false); };
            helper.Completed += (o, e) => { Assert.Equals(e.AuthorizationCode, authorizationCode);};

            helper.OnResponseReceived(uri);
        }
    }
}
