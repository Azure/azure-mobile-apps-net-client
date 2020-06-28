// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Push
{
    [TestClass]
    public class Push_Test
    {
        const string DefaultChannelUri = "http://channelUri.com/a b";
        const string DefaultServiceUri = MobileAppUriValidator.DummyMobileApp;
        const string RegistrationsPath = "push/registrations";
        const string InstallationsPath = "push/installations";
        const string DefaultRegistrationId = "7313155627197174428-6522078074300559092-1";

        [TestMethod]
        public async Task DeleteInstallationAsync()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.NoContent);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.DeleteInstallationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceInvalidOperationException))]
        public async Task DeleteInstallationAsync_Error()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);
            await pushHttpClient.DeleteInstallationAsync();
        }
    }
}
