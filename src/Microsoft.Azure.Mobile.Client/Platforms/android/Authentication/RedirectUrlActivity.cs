// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace Microsoft.WindowsAzure.MobileServices
{
    [Activity(Name = "com.microsoft.windowsazure.mobileservices.authentication.RedirectUrlActivity")]
    public class RedirectUrlActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            global::Android.Net.Uri uri_android = Intent.Data;

            if (uri_android != null)
            {
                // Convert Android.Net.Url to C#/netxf/BCL System.Uri - common API
                Uri uri_netfx = new Uri(uri_android.ToString());

                // load redirect_url Page for parsing
                MobileServiceUIAuthentication.CurrentAuthenticator?.OnPageLoading(uri_netfx);
            }

            this.Finish();
        }
    }
}
