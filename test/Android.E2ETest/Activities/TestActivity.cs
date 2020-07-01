// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.App;
using Android.E2ETest;
using Android.OS;
using Android.Widget;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    [Activity]
    public class TestActivity : Activity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.activity_test);

            Title = Intent.GetStringExtra ("name");

            FindViewById<TextView> (Resource.Id.Description).Text = Intent.GetStringExtra ("desc");
            FindViewById<TextView> (Resource.Id.Log).Text = Intent.GetStringExtra ("log");
        }
    }
}
