// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Test setup UI page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            string mobileServiceRuntimeURL = ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] as string;
            string tags = ApplicationData.Current.LocalSettings.Values["MobileServiceTags"] as string;

            txtRuntimeUri.Text = mobileServiceRuntimeURL ?? "";
            txtTags.Text = tags ?? "";

            Loaded += (s, e) => btnE2ETests.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Start the end to end test run.
        /// </summary>
        /// <param name="sender">Start button.</param>
        /// <param name="e">Event arguments.</param>
        private void ExecuteE2ETests(object sender, RoutedEventArgs e)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            App.Harness.Settings.TagExpression = txtTags.Text;

            ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            ApplicationData.Current.LocalSettings.Values["MobileServiceTags"] = txtTags.Text;

            Frame.Navigate(typeof(TestPage));
        }

        /// <summary>
        /// Loads the login test page.
        /// </summary>
        /// <param name="sender">Login button.</param>
        /// <param name="e">Event arguments.</param>
        private void ExecuteLoginTests(object sender, RoutedEventArgs e)
        {
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;

            Frame.Navigate(typeof(LoginPage));
        }
    }
}
