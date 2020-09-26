// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Platform-specific Login Tests for the Windows Store 8.1 and Windows Phone 8.1 platform.
    /// </summary>
    public class LoginTests
    {
        private const string RefreshUser400ErrorMessage = "Refresh failed with a 400 Bad Request error. The identity provider does not support refresh, or the user is not logged in with sufficient permission.";

        private const string GoogleTokenRevocationURI = @"https://accounts.google.com/o/oauth2/revoke?token={0}";

        private const string uriScheme = "zumoe2etestapp";

        internal static MobileServiceClient client;

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> 
        /// functionality of MicrosoftAccount, with <see cref="MobileServiceTable"/> CRUD operations
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestMicrosoftAccountRefreshUserAsync(bool useSingleSignOn)
        {
            await TestCRUDAsync("Public", tableRequiresAuthentication: false, userIsAuthenticated: false);
            await TestCRUDAsync("Authorized", tableRequiresAuthentication: true, userIsAuthenticated: false);

            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, uriScheme, useSingleSignOn);

            // save user.MobileServiceAuthenticationToken value for later use
            // because RefreshUserAsync() will override user.MobileServiceAuthenticationToken
            // in single sign-on scenario
            string authToken = user.MobileServiceAuthenticationToken;

            await TestCRUDAsync("Public", tableRequiresAuthentication: false, userIsAuthenticated: true);
            await TestCRUDAsync("Authorized", tableRequiresAuthentication: true, userIsAuthenticated: true);

            MobileServiceUser refreshedUser = await client.RefreshUserAsync();
            string refreshedAuthToken = refreshedUser.MobileServiceAuthenticationToken;

            Assert.AreEqual(user.UserId, refreshedUser.UserId);
            Assert.AreNotEqual(authToken, refreshedAuthToken);

            await TestLogoutAsync();
            await TestCRUDAsync("Authorized", tableRequiresAuthentication: true, userIsAuthenticated: false);

            return string.Format("User Login and Refresh succeeded. UserId: {0} Token: {1}", refreshedUser.UserId, refreshedUser.MobileServiceAuthenticationToken);
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of WindowsAzureActiveDirectory
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestAADRefreshUserAsync(bool useSingleSignOn)
        {
            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, uriScheme, useSingleSignOn,
                new Dictionary<string, string>()
                {
                    { "response_type", "code id_token" }
                });

            // save user.MobileServiceAuthenticationToken value for later use
            // because RefreshUserAsync() will override user.MobileServiceAuthenticationToken
            // in single sign-on scenario
            string authToken = user.MobileServiceAuthenticationToken;

            MobileServiceUser refreshedUser = await client.RefreshUserAsync();
            string refreshedAuthToken = refreshedUser.MobileServiceAuthenticationToken;

            Assert.AreEqual(user.UserId, refreshedUser.UserId);
            Assert.AreNotEqual(authToken, refreshedAuthToken);

            return string.Format("User Login and Refresh succeeded. UserId: {0} Token: {1}", refreshedUser.UserId, refreshedUser.MobileServiceAuthenticationToken);
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of Facebook
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestFacebookRefreshUserAsync(bool useSingleSignOn)
        {
            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.Facebook, uriScheme, useSingleSignOn);

            try
            {
                await client.RefreshUserAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                Assert.AreEqual(RefreshUser400ErrorMessage, ex.Message);
                return string.Format("User Login succeeded. User Refresh is not supported by Facebook. UserId: {0} Token: {1}", user.UserId, user.MobileServiceAuthenticationToken);
            }

            Assert.Fail("RefreshAsync() should throw 400 error on Facebook account.");
            return string.Empty;
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of Twitter
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestTwitterRefreshUserAsync(bool useSingleSignOn)
        {
            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.Twitter, uriScheme, useSingleSignOn);

            try
            {
                await client.RefreshUserAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                Assert.AreEqual(RefreshUser400ErrorMessage, ex.Message);
                return string.Format("User Login succeeded. User Refresh is not supported by Twitter. UserId: {0} Token: {1}", user.UserId, user.MobileServiceAuthenticationToken);
            }

            Assert.Fail("RefreshAsync() should throw 400 error on Twitter account.");
            return string.Empty;
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of Google
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestGoogleRefreshUserAsync(bool useSingleSignOn)
        {
            // Firstly, login user with Google account with offline permission
            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.Google, uriScheme, useSingleSignOn,
                new Dictionary<string, string>()
                {
                    { "access_type", "offline" }
                });

            // save user.MobileServiceAuthenticationToken value for later use
            // because RefreshUserAsync() will override user.MobileServiceAuthenticationToken
            // in single sign-on scenario
            string authToken = user.MobileServiceAuthenticationToken;

            // Secondly, refresh user using refresh token
            MobileServiceUser refreshedUser = await client.RefreshUserAsync();
            string refreshedAuthToken = refreshedUser.MobileServiceAuthenticationToken;

            Assert.AreEqual(user.UserId, refreshedUser.UserId);
            Assert.AreNotEqual(authToken, refreshedAuthToken);

            return string.Format("User Login and Refresh succeeded. UserId: {0} Token: {1}", refreshedUser.UserId, refreshedUser.MobileServiceAuthenticationToken);
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> and <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of Google
        /// </summary>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> TestGoogleRefreshUserAsyncWithTokenRecovation(bool useSingleSignOn)
        {
            // Firstly, login user with Google account with offline permission
            MobileServiceUser user = await LoginAsync(MobileServiceAuthenticationProvider.Google, uriScheme, useSingleSignOn,
                new Dictionary<string, string>()
                {
                    { "access_type", "offline" }
                });

            // save user.MobileServiceAuthenticationToken value for later use
            // because RefreshUserAsync() will override user.MobileServiceAuthenticationToken
            // in single sign-on scenario
            string authToken = user.MobileServiceAuthenticationToken;

            // Secondly, refresh user using refresh token
            MobileServiceUser refreshedUser = await client.RefreshUserAsync();
            string refreshedAuthToken = refreshedUser.MobileServiceAuthenticationToken;

            Assert.AreEqual(user.UserId, refreshedUser.UserId);
            Assert.AreNotEqual(authToken, refreshedAuthToken);

            // Then, get refresh token from /.auth/me endpoint of the backend
            string googleAccessToken = await GetGoogleAccessToken(client.MobileAppUri.AbsoluteUri, refreshedUser.MobileServiceAuthenticationToken);

            // Next, revoke Google access token, which would subsequently revoke Google refresh token
            _ = await RevokeGoogleAccessToken(googleAccessToken);

            // Next, refresh user with the refresh token which just got revoked.
            // Because of the revocation, refresh will throw 403 Forbidden exception
            MobileServiceInvalidOperationException exception = null;
            try
            {
                await client.RefreshUserAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
            if (exception == null)
            {
                Assert.Fail("RefreshUserAsync API should throw 403 exception because of access token revocation.");
            }
            if (exception.Response.StatusCode != System.Net.HttpStatusCode.Forbidden)
            {
                Assert.Fail("RefreshUserAsync API throws " + exception.Response.StatusCode + " exception instead of 403 exception.");
            }

            // Last, login user with Google offline scope again
            MobileServiceUser sameUser = await LoginAsync(MobileServiceAuthenticationProvider.Google, uriScheme, useSingleSignOn,
                new Dictionary<string, string>()
                {
                    { "access_type", "offline" }
                });
            string authToken2 = sameUser.MobileServiceAuthenticationToken;

            // Refresh user should work this time
            MobileServiceUser refreshedSameUser = await client.RefreshUserAsync();
            string refreshedAuthToken2 = refreshedSameUser.MobileServiceAuthenticationToken;

            Assert.AreEqual(refreshedSameUser.UserId, sameUser.UserId);
            Assert.AreNotEqual(authToken2, refreshedAuthToken2);

            return string.Format("User Login and Refresh succeeded. UserId: {0} Token: {1}", refreshedSameUser.UserId, refreshedSameUser.MobileServiceAuthenticationToken);
        }

        private static Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, string uriScheme, bool singleSignOn, IDictionary<string, string> parameters = null)
        {
            if (singleSignOn)
            {
                return client.LoginAsync(provider, singleSignOn, parameters);
            }
            else
            {
                return client.LoginAsync(provider, uriScheme, parameters);
            }
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.RefreshUserAsync"/> functionality of Google
        /// </summary>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained after refresh
        /// </returns>
        private static async Task<string> GetGoogleAccessToken(string mobileServiceUri, string mobileServiceAuthenticationToken)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(mobileServiceUri + ".auth/me")
            };
            request.Headers.Add("zumo-api-version", "2.0.0");
            request.Headers.Add("x-zumo-auth", mobileServiceAuthenticationToken);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            string responseString = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseString))
            {
                JToken jtoken = JToken.Parse(responseString);
                return (string)jtoken[0]["access_token"];
            }
            else
            {
                throw new Exception("Failed to fetch refresh token");
            }
        }

        private static async Task<bool> RevokeGoogleAccessToken(string googleAccessToken)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(GoogleTokenRevocationURI, googleAccessToken))
            };
            HttpResponseMessage response = await httpClient.SendAsync(request);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.RefreshUserAsync"/> functionality on the platform.
        /// </summary>
        /// <param name="provider">
        /// The provider with which to login.
        /// </param>
        /// <param name="useSingleSignOn">
        /// use single sign-on to login the user.
        /// </param>
        /// <returns>
        /// The UserId and MobileServiceAuthentication token obtained from logging in.
        /// </returns>
        public static async Task<string> TestRefreshUserAsync(MobileServiceAuthenticationProvider provider, bool useSingleSignOn)
        {
            string result;
            switch (provider)
            {
                case MobileServiceAuthenticationProvider.MicrosoftAccount:
                    result = await TestMicrosoftAccountRefreshUserAsync(useSingleSignOn);
                    break;
                case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
                    result = await TestAADRefreshUserAsync(useSingleSignOn);
                    break;
                case MobileServiceAuthenticationProvider.Google:
                    result = await TestGoogleRefreshUserAsync(useSingleSignOn);
                    break;
                case MobileServiceAuthenticationProvider.Facebook:
                    result = await TestFacebookRefreshUserAsync(useSingleSignOn);
                    break;
                case MobileServiceAuthenticationProvider.Twitter:
                    result = await TestTwitterRefreshUserAsync(useSingleSignOn);
                    break;
                default:
                    result = "MobileServiceAuthenticationProvider is not recognized.";
                    break;
            }
            return result;
        }

        private static async Task<string> TestLogoutAsync()
        {
            await client.LogoutAsync();
            return string.Format("Logged out. Current logged-in client: {0}", client.CurrentUser == null ? "<<NULL>>" : client.CurrentUser.UserId);
        }

        private static async Task<string> TestCRUDAsync(string tableName, bool tableRequiresAuthentication, bool userIsAuthenticated)
        {
            bool crudShouldWork = (tableRequiresAuthentication && userIsAuthenticated) || !tableRequiresAuthentication;
            _ = client.CurrentUser;
            var table = client.GetTable(tableName);
            var item = new JObject
            {
                { "name", "John Doe" }
            };
            string id;
            Dictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                {"userIsAuthenticated", userIsAuthenticated.ToString().ToLowerInvariant() },
            };

            // Attempt to insert 
            try
            {
                JToken inserted = await table.InsertAsync(item, queryParameters);
                item = (JObject)inserted;
                Debug.WriteLine(string.Format("Inserted item: {0}", item));
                id = item["id"].Value<string>();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                // Store the ID for other tests.
                id = "1";
                item["id"] = 1;
                ValidateExpectedFailure("Insert", tableName, crudShouldWork, tableRequiresAuthentication, e);
            }

            // Attempt to update 
            try
            {
                item["name"] = "Jane Roe";
                JToken updated = await table.UpdateAsync(item, queryParameters);
                Debug.WriteLine(string.Format("Updated item: {0}", item));
            }
            catch (MobileServiceInvalidOperationException e)
            {
                ValidateExpectedFailure("Update", tableName, crudShouldWork, tableRequiresAuthentication, e);
            }

            // Attempt to read data
            JObject updatedItem = null;
            try
            {
                updatedItem = (JObject)await table.LookupAsync(id, queryParameters);
                Debug.WriteLine(string.Format("Retrieved item via lookup: {0}", updatedItem));
            }
            catch (MobileServiceInvalidOperationException e)
            {
                ValidateExpectedFailure("Lookup", tableName, crudShouldWork, tableRequiresAuthentication, e);
            }

            // Attempt to read data with filter
            try
            {
                JToken items = null;
                items = await table.ReadAsync(string.Format("$filter=id eq '{0}'", id), queryParameters);
                Debug.WriteLine(string.Format("Retrieved items via Read: {0}", items));
                if (((JArray)items).Count != 1)
                {
                    string errorMessage = "Error, query should have returned exactly one item";
                    throw new Exception(errorMessage);
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                ValidateExpectedFailure("Read", tableName, crudShouldWork, tableRequiresAuthentication, e);
            }

            // Attempt to delete the data
            try
            {
                var itemToDelete = updatedItem ?? item;
                JToken deletedItem = await table.DeleteAsync(itemToDelete, queryParameters);
                Debug.WriteLine(string.Format("Deleted item: {0}", itemToDelete));
            }
            catch (MobileServiceInvalidOperationException e)
            {
                ValidateExpectedFailure("Delete", tableName, crudShouldWork, tableRequiresAuthentication, e);
            }

            return "Test complete";
        }

        private static void ValidateExpectedFailure(string attemptedAction, string tableName, bool crudShouldWork, bool tableRequiresAuth, MobileServiceInvalidOperationException innerException)
        {
            if (crudShouldWork)
            {
                string errorMessage;
                if (!tableRequiresAuth)
                {
                    errorMessage = string.Format("Anonymous {0} to unprotected table {1} should have succeeded. Instead, found Status Code: {2}. Reason: {3}.", attemptedAction, tableName, innerException.Response.StatusCode, innerException.Response.ReasonPhrase);
                }
                else
                {
                    errorMessage = string.Format("Authenticated {0} to protected table {1} should have succeeded. Instead, found Status Code: {2}. Reason: {3}.", attemptedAction, tableName, innerException.Response.StatusCode, innerException.Response.ReasonPhrase);
                }
                throw new Exception(errorMessage, innerException);
            }
        }

        /// <summary>
        /// Utility method that can be used to execute a test.  It will capture any exceptions throw
        /// during the execution of the test and return a message with details of the exception thrown.
        /// </summary>
        /// <param name="testName">The name of the test being executed.
        /// </param>
        /// <param name="test">A test to execute.
        /// </param>
        /// <returns>
        /// Either the result of the test if the test passed, or a message with the exception
        /// that was thrown.
        /// </returns>
        public static async Task<string> ExecuteTest(string testName, Func<Task<string>> test)
        {
            bool didPass = false;

            if (client == null)
            {
                App.Harness.Settings.Custom.TryGetValue("MobileServiceRuntimeUrl", out string appUrl);

                client = new MobileServiceClient(appUrl);

                App.LoginMobileService = client;
            }

            string resultText;
            try
            {
                resultText = await test();
                didPass = true;
            }
            catch (Exception exception)
            {
                resultText = string.Format("ExceptionType: {0} Message: {1} StackTrace: {2}",
                                               exception.GetType().ToString(),
                                               exception.Message,
                                               exception.StackTrace);
            }

            return string.Format("Test '{0}' {1}.\n{2}",
                                 testName,
                                 didPass ? "PASSED" : "FAILED",
                                 resultText);
        }
    }
}
