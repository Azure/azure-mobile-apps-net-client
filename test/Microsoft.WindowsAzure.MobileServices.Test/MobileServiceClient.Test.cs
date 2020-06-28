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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [TestClass]
    public class MobileServiceClient_Test
    {
        [TestMethod]
        public void Constructor_Basic()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.AreEqual(MobileAppUriValidator.DummyMobileApp, service.MobileAppUri.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullThrows()
        {
            var service = new MobileServiceClient(mobileAppUri: (string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullUriThrows()
        {
            var service = new MobileServiceClient(mobileAppUri: (Uri)null);
        }

        [TestMethod]
        public void Constructor_Basic_NoTrailingSlash()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileAppUriWithFolderWithoutTralingSlash);
            Assert.IsNotNull(service.MobileAppUri);
            Assert.IsTrue(service.MobileAppUri.IsAbsoluteUri);
            Assert.IsTrue(service.MobileAppUri.AbsoluteUri.EndsWith("/", StringComparison.InvariantCulture));
            Assert.IsNotNull(service.HttpClient);
        }

        [TestMethod]
        public void InstallationId_IsAvailable()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.IsNotNull(service.InstallationId);
        }

        [TestMethod]
        public async Task Constructor_SingleHttpHandler()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, handlers: hijack);
            var validator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("[]");
            JToken response = await service.GetTable("foo").ReadAsync("bar");
            StringAssert.StartsWith(hijack.Request.RequestUri.ToString(), validator.TableBaseUri);
        }

        [TestMethod]
        public void Constructor_DoesNotRewireHandlers()
        {
            var innerHandler = new TestHttpHandler();
            var wiredHandler = new TestHttpHandler();
            wiredHandler.InnerHandler = innerHandler;

            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, handlers: wiredHandler);
            Assert.AreEqual(wiredHandler.InnerHandler, innerHandler);
        }

        [TestMethod]
        public async Task Constructor_MultipleHttpHandlers()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            var firstBeforeMessage = "Message before 1";
            var firstAfterMessage = "Message after 1";
            var secondBeforeMessage = "Message before 2";
            var secondAfterMessage = "Message after 2";

            var firstHandler = new ComplexDelegatingHandler(firstBeforeMessage, firstAfterMessage);
            var secondHandler = new ComplexDelegatingHandler(secondBeforeMessage, secondAfterMessage);
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, new HttpMessageHandler[] { firstHandler, secondHandler, hijack });

            Assert.AreSame(hijack, secondHandler.InnerHandler);
            Assert.AreSame(secondHandler, firstHandler.InnerHandler);

            ComplexDelegatingHandler.ClearStoredMessages();
            hijack.SetResponseContent("[]");
            JToken response = await service.GetTable("foo").ReadAsync("bar");

            var storedMessages = new List<string>(ComplexDelegatingHandler.AllMessages);
            Assert.AreEqual(4, storedMessages.Count);
            Assert.AreEqual(firstBeforeMessage, storedMessages[0]);
            Assert.AreEqual(secondBeforeMessage, storedMessages[1]);
            Assert.AreEqual(secondAfterMessage, storedMessages[2]);
            Assert.AreEqual(firstAfterMessage, storedMessages[3]);
        }

        [TestMethod]
        public async Task Logout_Basic()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            service.CurrentUser = new MobileServiceUser("123456");
            Assert.IsNotNull(service.CurrentUser);
            await service.LogoutAsync();
            Assert.IsNull(service.CurrentUser);
        }

        [TestMethod]
        public async Task StandardRequestFormat()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            service.CurrentUser = new MobileServiceUser("someUser");
            service.CurrentUser.MobileServiceAuthenticationToken = "Not rhubarb";

            hijack.SetResponseContent("[{\"id\":12,\"value\":\"test\"}]");
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.IsNotNull(hijack.Request.Headers.GetValues("X-ZUMO-INSTALLATION-ID").First());
            Assert.AreEqual("application/json", hijack.Request.Headers.Accept.First().MediaType);
            Assert.AreEqual("Not rhubarb", hijack.Request.Headers.GetValues("X-ZUMO-AUTH").First());
            Assert.IsNotNull(hijack.Request.Headers.GetValues("ZUMO-API-VERSION").First());

            string userAgent = hijack.Request.Headers.UserAgent.ToString();
            Assert.IsTrue(userAgent.Contains("ZUMO/5."));
            Assert.IsTrue(userAgent.Contains("version=5."));
        }

        [TestMethod]
        public async Task ErrorMessageConstruction()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            // Verify the error message is correctly pulled out
            hijack.SetResponseContent("{\"error\":\"error message\",\"other\":\"donkey\"}");
            hijack.Response.StatusCode = HttpStatusCode.Unauthorized;
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
            }

            // Verify all of the exception parameters
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
                Assert.AreEqual(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                StringAssert.Contains(ex.Response.Content.ReadAsStringAsync().Result, "donkey");
                StringAssert.StartsWith(ex.Request.RequestUri.ToString(), mobileAppUriValidator.TableBaseUri);
                Assert.AreEqual("YOU SHALL NOT PASS.", ex.Response.ReasonPhrase);
            }

            // If no error message in the response, we'll use the
            // StatusDescription instead
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            hijack.SetResponseContent("{\"other\":\"donkey\"}");

            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("The request could not be completed.  (YOU SHALL NOT PASS.)", ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.DeleteAsync(new ToDoWithSystemPropertiesType("abc"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task InsertAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await  table.InsertAsync(new ToDoWithSystemPropertiesType("abc"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UpdateAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.UpdateAsync(new ToDoWithSystemPropertiesType("abc"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LookupAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.LookupAsync("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ReadAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.Where(t => t.String == "abc").ToListAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task PurgeAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.PurgeAsync(table.Where(t => t.String == "abc"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task PullAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await table.PullAsync(null, table.Where(t => t.String == "abc"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetTableThrowsWithNullTable()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            service.GetTable(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetTableThrowsWithEmptyStringTable()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            service.GetTable("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InvokeCustomApiThrowsForNullApiName()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.InvokeApiAsync("", null);
        }

        [TestMethod]
        public async Task InvokeCustomAPISimple()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, expected.Id);
        }

        [TestMethod]
        public async Task InvokeCustomAPISimpleJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [TestMethod]
        public async Task InvokeCustomAPIPost()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [TestMethod]
        public async Task InvokeCustomAPIPostJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [TestMethod]
        public async Task InvokeCustomAPIPostJTokenBooleanPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(content, "true");
                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue(true));
        }

        [TestMethod]
        public async Task InvokeCustomAPIPostJTokenNullPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(content, "null");
                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue((object)null));
        }


        [TestMethod]
        public async Task InvokeCustomAPIGet()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, expected.Id);
        }

        [TestMethod]
        public async Task InvokeApiAsync_DoesNotAppendApiPath_IfApiStartsWithSlash()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.SetResponseContent("{\"id\":3}");

            await service.InvokeApiAsync<IntType>("/calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/calculator/add");
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [TestMethod]
        public async Task InvokeCustomAPIGetJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [TestMethod]
        public async Task InvokeCustomAPIGetWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [TestMethod]
        public async Task InvokeCustomAPIGetWithParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [TestMethod]
        public async Task InvokeCustomAPIGetWithODataParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" }, { "$take", "1" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            StringAssert.Contains(hijack.Request.RequestUri.Query, "?%24select=one%2Ctwo&%24take=1");
        }

        [TestMethod]
        public async Task InvokeCustomAPIGetWithODataParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);
            StringAssert.Contains(hijack.Request.RequestUri.Query, "?%24select=one%2Ctwo");
        }

        [TestMethod]
        public async Task InvokeCustomAPIPostWithBody()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body, HttpMethod.Post, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNotNull(hijack.Request.Content);
        }

        [TestMethod]
        public async Task InvokeCustomAPIPostWithBodyJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body, HttpMethod.Post, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNotNull(hijack.Request.Content);
        }

        [TestMethod]
        public async Task InvokeCustomAPIResponse()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add?a=1&b=2", null, HttpMethod.Post, null, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            StringAssert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [TestMethod]
        public async Task InvokeCustomAPIResponseWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", null, HttpMethod.Post, null, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNull(hijack.Request.Content);
            StringAssert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [TestMethod]
        public async Task InvokeCustomAPIResponseWithParamsBodyAndHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            HttpContent content = new StringContent("{\"test\" : \"one\"}", Encoding.UTF8, "application/json");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            var myHeaders = new Dictionary<string, string>() { { "x-zumo-test", "test" } };

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", content, HttpMethod.Post, myHeaders, myParams);

            Assert.AreEqual(myHeaders.Count, 1); // my headers should not be modified
            Assert.AreEqual(myHeaders["x-zumo-test"], "test");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.Headers.GetValues("x-zumo-test").First(), "test");
            Assert.IsNotNull(hijack.Request.Content);
            Assert.AreEqual(hijack.Request.RequestUri.Query, "?a=1&b=2");
            StringAssert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [TestMethod]
        public async Task InvokeCustomAPIWithEmptyStringResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            hijack.Response.Content = new StringContent("", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("testapi");
            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("testapi"));
            Assert.AreEqual(expected, null);
        }

        [TestMethod]
        public async Task InvokeGenericCustomAPIWithNullResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            hijack.Response.Content = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            IntType expected = await service.InvokeApiAsync<IntType>("testapi");
            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("testapi"));
            Assert.AreEqual(expected, null);
        }

        [TestMethod]
        public async Task InvokeCustomAPI_ErrorWithJsonObject()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("{ error: \"message\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [TestMethod]
        public async Task InvokeCustomAPI_ErrorWithJsonString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("\"message\"", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [TestMethod]
        public async Task InvokeCustomAPI_ErrorWithString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, "text/html");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [TestMethod]
        public async Task InvokeCustomAPI_ErrorStringAndNoContentType()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, null);
            hijack.Response.Content.Headers.ContentType = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "The request could not be completed.  (Bad Request)");
            }
        }

        [TestMethod]
        public async Task InvokeApiJsonOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AJ", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName");

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", JObject.Parse("{\"a\":1}"));

            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AJ,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", null, HttpMethod.Delete, dic);
        }

        [TestMethod]
        public async Task InvokeApiTypedOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AT", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName");

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 });

            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AT,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Put, dic);
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String()
        {
            return this.ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName"));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken()
        {
            return this.ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}")));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", null, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}"), HttpMethod.Put, new Dictionary<string, string> { { "a", "b" } }));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_NoQueryParams()
        {
            var content = new StringContent("hello world", Encoding.UTF8, "text/plain");
            return this.ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", content, HttpMethod.Post, null, null));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_WithQueryParams()
        {
            var content = new StringContent("hello world", Encoding.UTF8, "text/plain");
            return this.ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", content, HttpMethod.Post, null, new Dictionary<string, string> { { "a", "b" } }));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String()
        {
            return this.ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType>("apiName"));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T()
        {
            return this.ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }));
        }

        [TestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        [TestMethod]
        public async Task LoginAsync_UserEnumTypeProvider()
        {
            JObject token = new JObject();
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
            string expectedUri = appUrl + ".auth/login/microsoftaccount";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);

            await client.LoginAsync(provider, token);

            Assert.AreEqual(expectedUri, hijack.Request.RequestUri.OriginalString);
        }

        [TestMethod]
        public async Task LoginAsync_UserStringTypeProvider()
        {
            JObject token = new JObject();
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
            string expectedUri = appUrl + ".auth/login/microsoftaccount";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);

            await client.LoginAsync(provider.ToString(), token);

            Assert.AreEqual(expectedUri, hijack.Request.RequestUri.OriginalString);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RefreshUserAsync_Throws_WhenMobileServiceUserIsNotSet()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;

            TestHttpHandler hijack = new TestHttpHandler();

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = null;
            await client.RefreshUserAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RefreshUserAsync_Throws_WhenMobileServiceUserHasNoAuthToken()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId);
            await client.RefreshUserAsync();
        }

        [TestMethod]
        public async Task RefreshUserAsync_NonAlternateLoginUri()
        {
            await RefreshUserAsync_Setup();
        }

        [TestMethod]
        public async Task RefreshUserAsync_AlternateLoginUri()
        {
            await RefreshUserAsync_Setup(alternateLoginUri: "https://www.testalternatelogin.com/");
        }

        private static async Task RefreshUserAsync_Setup(string alternateLoginUri = null)
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string newAuthToken = "new-auth-token";
            string userId = "sid:xxxxxxxxxxxxxxxxx";
            string responseContent = "{\"authenticationToken\":\"" + newAuthToken + "\",\"user\":{\"userId\":\"" + userId + "\"}}";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.Response.Content = new StringContent(responseContent);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            string refreshUrl;
            if (!string.IsNullOrEmpty(alternateLoginUri))
            {
                refreshUrl = alternateLoginUri + ".auth/refresh";
                client.AlternateLoginHost = new Uri(alternateLoginUri);
            }
            else
            {
                refreshUrl = appUrl + ".auth/refresh";
            }
            MobileServiceUser user = await client.RefreshUserAsync();

            Assert.AreEqual(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken),
                hijack.Request.Headers.GetValues(MobileServiceHttpClient.ZumoFeaturesHeader).FirstOrDefault());
            Assert.AreEqual(hijack.Request.RequestUri.OriginalString, refreshUrl);
            Assert.AreEqual(newAuthToken, user.MobileServiceAuthenticationToken);
            Assert.AreEqual(userId, user.UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceInvalidOperationException))]
        public async Task RefreshUserAsync_Throws_On_BadRequest()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.BadRequest);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await client.RefreshUserAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceInvalidOperationException))]
        public async Task RefreshUserAsync_Throws_On_Unauthorized()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.Unauthorized);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await client.RefreshUserAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceInvalidOperationException))]
        public async Task RefreshUserAsync_Throws_On_Forbidden()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.Forbidden);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await client.RefreshUserAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceInvalidOperationException))]
        public async Task RefreshUserAsync_Throws_On_InternalServerError()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.InternalServerError);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await client.RefreshUserAsync();
        }

        [TestMethod]
        public void AddFeaturesHeader_RequestHeadersIsNull()
        {
            IDictionary<string, string> requestedHeaders = null;
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.None);
            Assert.IsNull(requestedHeaders);

            requestedHeaders = null;
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.RefreshToken);
            Assert.AreEqual(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken), requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        [TestMethod]
        public void AddFeaturesHeader_RequestHeadersIsNotNull()
        {
            IDictionary<string, string> requestedHeaders = new Dictionary<string, string>();
            requestedHeaders.Add("key1", "value1");
            requestedHeaders.Add("key2", "value2");
            Assert.AreEqual(2, requestedHeaders.Count);

            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.None);
            Assert.AreEqual(2, requestedHeaders.Count);

            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.RefreshToken | MobileServiceFeatures.Offline);
            Assert.AreEqual(3, requestedHeaders.Count);
            Assert.AreEqual("OL,RT", requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        [TestMethod]
        public void AddFeaturesHeader_ZumoFeaturesHeaderAlreadyExists()
        {
            IDictionary<string, string> requestedHeaders = new Dictionary<string, string>();
            requestedHeaders.Add("key1", "value1");
            requestedHeaders.Add("key2", "value2");
            requestedHeaders.Add(MobileServiceHttpClient.ZumoFeaturesHeader, EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken));
            Assert.AreEqual(3, requestedHeaders.Count);

            // AddFeaturesHeader won't add anything because ZumoFeaturesHeader already exists
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.Offline);
            Assert.AreEqual(3, requestedHeaders.Count);
            Assert.AreEqual(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken), requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        private async Task ValidateFeaturesHeader(string expectedFeaturesHeader, Func<IMobileServiceClient, Task> operation)
        {
            TestHttpHandler hijack = new TestHttpHandler();
            bool validationDone = false;
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual(expectedFeaturesHeader, request.Headers.GetValues("X-ZUMO-FEATURES").First());
                validationDone = true;
                return Task.FromResult(request);
            };

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"id\":3}");
            await operation(service);
            Assert.IsTrue(validationDone);
        }
    }
}
