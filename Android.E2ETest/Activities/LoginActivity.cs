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

        private EditText uriText, tagsText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_login);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            this.uriText = FindViewById<EditText>(Resource.Id.ServiceUri);
            this.tagsText = FindViewById<EditText>(Resource.Id.ServiceTags);

            this.uriText.Text = prefs.GetString(Keys.MobileServiceUri, null);
            this.tagsText.Text = prefs.GetString(Keys.TagExpression, null);

            FindViewById<Button>(Resource.Id.RunTests).Click += OnClickRunTests;

            string autoStart = ReadSettingFromIntentOrDefault(Keys.AutoStart, "false");
            if (autoStart != null && string.Equals(autoStart, "true", StringComparison.CurrentCultureIgnoreCase))
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
    }
}
