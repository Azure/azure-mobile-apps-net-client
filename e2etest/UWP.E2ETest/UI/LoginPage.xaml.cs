// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Uri)
            {
                LoginTests.client.ResumeWithURL(e.Parameter as Uri);
            }
        }

        private async void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = sender as Button;
            if (buttonClicked != null)
            {
                String testName = string.Empty;
                MobileServiceAuthenticationProvider provider =
                    MobileServiceAuthenticationProvider.MicrosoftAccount;

                bool useSingleSignOn = UseSingleSignOnCheckBox.IsChecked.Value;

                switch (buttonClicked.Name)
                {
                    case "MicrosoftAccountButton":
                        provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
                        testName = "Microsoft Account Login and Refresh User";
                        break;
                    case "FacebookButton":
                        provider = MobileServiceAuthenticationProvider.Facebook;
                        testName = "Facebook Login and Refresh Refresh User";
                        break;
                    case "TwitterButton":
                        provider = MobileServiceAuthenticationProvider.Twitter;
                        testName = "Twitter Login and Refresh User";
                        break;
                    case "GoogleButton":
                        provider = MobileServiceAuthenticationProvider.Google;
                        testName = "Google Login and Refresh User";
                        break;
                    case "AzureActiveDirectoryButton":
                        provider = MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory;
                        testName = "AAD Login and Refresh User";
                        break;
                    default:
                        break;
                }

                TestResultsTextBlock.Text = await LoginTests.ExecuteTest(testName, () => LoginTests.TestRefreshUserAsync(provider, useSingleSignOn));
            }
        }
    }
}
