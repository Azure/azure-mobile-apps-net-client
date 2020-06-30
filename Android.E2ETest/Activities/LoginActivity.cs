// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.E2ETest;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    [Activity(Name="microsoft.windowsazure.mobile.android.test.LoginActivity", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        static class Keys
        {
            public const string MobileServiceUri = "MobileServiceUri";
            public const string TagExpression = "TagExpression";
            public const string AutoStart = "AutoStart";
            public const string RuntimeVersion = "RuntimeVersion";
            public const string StorageUrl = "storageUrl";
            public const string StorageSasToken = "storageSasToken";
        }

        private const string RefreshUser400ErrorMessage = "Refresh failed with a 400 Bad Request error. The identity provider does not support refresh, or the user is not logged in with sufficient permission.";

        private const string uriScheme = "zumoe2etestapp";

        private EditText uriText, tagsText;
        private TextView loginTestResult;

        /// <summary>
        /// Detect if on Android emulator or real device
        /// </summary>
        private bool onEmulator
        {
            get
            {
                if (Build.Fingerprint != null)
                {
                    if (Build.Fingerprint.Contains("vsemu") || Build.Fingerprint.Contains("vbox") || Build.Fingerprint.Contains("generic"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_login);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            this.uriText = FindViewById<EditText>(Resource.Id.ServiceUri);
            this.tagsText = FindViewById<EditText>(Resource.Id.ServiceTags);
            this.loginTestResult = FindViewById<TextView>(Resource.Id.LoginTestResult);

            this.uriText.Text = prefs.GetString(Keys.MobileServiceUri, null);
            this.tagsText.Text = prefs.GetString(Keys.TagExpression, null);

            FindViewById<Button>(Resource.Id.RunTests).Click += OnClickRunTests;
            FindViewById<Button>(Resource.Id.MSALogin).Click += CatchInvalidOperationExceptions(OnClickMicrosoftAccountLoginAndRefresh);
            FindViewById<Button>(Resource.Id.AADLogin).Click += CatchInvalidOperationExceptions(OnClickAADLoginAndRefresh);
            FindViewById<Button>(Resource.Id.GoogleLogin).Click += CatchInvalidOperationExceptions(OnClickGoogleLoginAndRefresh);
            FindViewById<Button>(Resource.Id.FacebookLogin).Click += CatchInvalidOperationExceptions(OnClickFacebookLoginAndRefresh);
            FindViewById<Button>(Resource.Id.TwitterLogin).Click += CatchInvalidOperationExceptions(OnClickTwitterLoginAndRefresh);

            string autoStart = ReadSettingFromIntentOrDefault(Keys.AutoStart, "false");
            if (autoStart != null && autoStart.ToLower() == "true")
            {
                TestConfig config = new TestConfig
                {
                    MobileServiceRuntimeUrl = ReadSettingFromIntentOrDefault(Keys.MobileServiceUri),
                    RuntimeVersion = ReadSettingFromIntentOrDefault(Keys.RuntimeVersion),
                    TagExpression = ReadSettingFromIntentOrDefault(Keys.TagExpression),
                    TestFrameworkStorageContainerUrl = ReadSettingFromIntentOrDefault(Keys.StorageUrl),
                    TestFrameworkStorageContainerSasToken = ReadSettingFromIntentOrDefault(Keys.StorageSasToken)
                };
                App.Harness.SetAutoConfig(config);
                RunTests();
            }
        }

        private EventHandler CatchInvalidOperationExceptions(Func<Task> origin)
        {
            return async (sender, eventArgs) =>
            {
                try
                {
                    await origin();
                }
                catch (InvalidOperationException e)
                {
                    string message = "Failed with exception: " + e.Message;
                    this.loginTestResult.Text = message;
                    System.Diagnostics.Debug.WriteLine(message);
                }
            };
        }

        private string ReadSettingFromIntentOrDefault(string key, string defaultValue = null)
        {
            string fromIntent = Intent.GetStringExtra(key);
            if (!string.IsNullOrWhiteSpace(fromIntent))
            {
                return fromIntent;
            }
            return defaultValue;
        }

        private void OnClickRunTests(object sender, EventArgs eventArgs)
        {
            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this))
            using (ISharedPreferencesEditor editor = prefs.Edit())
            {
                editor.PutString(Keys.MobileServiceUri, this.uriText.Text);
                editor.PutString(Keys.TagExpression, this.tagsText.Text);

                editor.Commit();
            }

            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = this.uriText.Text;
            App.Harness.Settings.TagExpression = this.tagsText.Text;

            RunTests();
        }

        private void RunTests()
        {
            Task.Factory.StartNew(App.Harness.RunAsync);

            Intent intent = new Intent(this, typeof(HarnessActivity));
            StartActivity(intent);
        }

        private async Task OnClickMicrosoftAccountLoginAndRefresh()
        {
            //this.loginTestResult.Text = string.Empty;

            //var client = new MobileServiceClient(this.uriText.Text);
            //var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.MicrosoftAccount, uriScheme);
            //string authToken = user.MobileServiceAuthenticationToken;
            //this.loginTestResult.Text = "MicrosoftAccount LoginAsync succeeded. UserId: " + user.UserId;

            //MobileServiceUser refreshedUser = await client.RefreshUserAsync();

            //Assert.AreEqual(user.UserId, refreshedUser.UserId);
            //Assert.AreNotEqual(authToken, refreshedUser.MobileServiceAuthenticationToken);

            //string message = "MicrosoftAccount LoginAsync succeeded. MicrosoftAccount RefreshAsync succeeded. UserId: " + user.UserId;
            //this.loginTestResult.Text = message;
            //System.Diagnostics.Debug.WriteLine(message);
        }

        private async Task OnClickAADLoginAndRefresh()
        {
            //this.loginTestResult.Text = string.Empty;

            //var client = new MobileServiceClient(this.uriText.Text);
            //var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, uriScheme,
            //    new Dictionary<string, string>()
            //    {
            //        { "response_type", "code id_token" }
            //    });
            //string authToken = user.MobileServiceAuthenticationToken;
            //this.loginTestResult.Text = "AAD LoginAsync succeeded. UserId: " + user.UserId;

            //MobileServiceUser refreshedUser = await client.RefreshUserAsync();

            //Assert.AreEqual(user.UserId, refreshedUser.UserId);
            //Assert.AreNotEqual(authToken, refreshedUser.MobileServiceAuthenticationToken);

            //string message = "AAD LoginAsync succeeded. AAD RefreshAsync succeeded. UserId: " + user.UserId;
            //this.loginTestResult.Text = message;
            //System.Diagnostics.Debug.WriteLine(message);
        }

        private async Task OnClickGoogleLoginAndRefresh()
        {
            //this.loginTestResult.Text = string.Empty;

            //var client = new MobileServiceClient(this.uriText.Text);
            //var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.Google, uriScheme,
            //    new Dictionary<string, string>()
            //    {
            //        { "access_type", "offline" }
            //    });
            //string authToken = user.MobileServiceAuthenticationToken;
            //this.loginTestResult.Text = "Google LoginAsync succeeded. UserId: " + user.UserId;

            //MobileServiceUser refreshedUser = await client.RefreshUserAsync();

            //Assert.AreEqual(user.UserId, refreshedUser.UserId);
            //Assert.AreNotEqual(authToken, refreshedUser.MobileServiceAuthenticationToken);

            //string message = "Google LoginAsync succeeded. Google RefreshAsync succeeded. UserId: " + user.UserId;
            //this.loginTestResult.Text = message;
            //System.Diagnostics.Debug.WriteLine(message);
        }

        private async Task OnClickFacebookLoginAndRefresh()
        {
            //this.loginTestResult.Text = string.Empty;

            //var client = new MobileServiceClient(this.uriText.Text);
            //var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook, uriScheme);
            //this.loginTestResult.Text = "Facebook LoginAsync succeeded. UserId: " + user.UserId;

            //try
            //{
            //    await client.RefreshUserAsync();
            //}
            //catch (MobileServiceInvalidOperationException ex)
            //{
            //    Assert.IsNotNull(ex.InnerException);
            //    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
            //    Assert.AreEqual(RefreshUser400ErrorMessage, ex.Message);
            //    string message = "Facebook LoginAsync succeeded. RefreshAsync is not supported by Facebook. UserId: " + user.UserId;
            //    this.loginTestResult.Text = message;
            //    System.Diagnostics.Debug.WriteLine(message);
            //    return;
            //}

            //Assert.Fail("RefreshAsync() should throw 400 error on Facebook account.");
        }

        private async Task OnClickTwitterLoginAndRefresh()
        {
            //var client = new MobileServiceClient(this.uriText.Text);
            //var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.Twitter, uriScheme);

            //try
            //{
            //    await client.RefreshUserAsync();
            //}
            //catch (MobileServiceInvalidOperationException ex)
            //{
            //    Assert.IsNotNull(ex.InnerException);
            //    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
            //    Assert.AreEqual(RefreshUser400ErrorMessage, ex.Message);
            //    string message = "Twitter LoginAsync succeeded. RefreshAsync is not supported by Twitter. UserId: " + user.UserId;
            //    this.loginTestResult.Text = message;
            //    System.Diagnostics.Debug.WriteLine(message);
            //    return;
            //}

            //Assert.Fail("RefreshAsync() should throw 400 error on Twitter account.");
        }
    }
}
