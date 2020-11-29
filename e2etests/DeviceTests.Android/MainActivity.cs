// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.App;
using Android.Content.PM;
using Android.OS;
using DeviceTests.Shared.Tests;
using System.Reflection;
using Xunit.Runners.UI;

namespace DeviceTests.Android
{
    [Activity(
        Name = "com.microsoft.azure.devicetests.MainActivity",
        Label = "@string/app_name",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Xamarin.Essentials.Platform.Init(this, bundle);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());
            AddTestAssembly(typeof(Shared_Tests).Assembly);
            AddExecutionAssembly(typeof(Shared_Tests).Assembly);

            base.OnCreate(bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}