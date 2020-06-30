using Foundation;
using iOS.E2ETest;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using MonoTouch.Dialog;
using UIKit;

namespace Microsoft.WindowsAzure.Mobile.iOS.Test
{
    class LoginViewController : DialogViewController
    {
        public LoginViewController()
            : base(UITableViewStyle.Grouped, null)
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            string mobileServiceUri = defaults.StringForKey(MobileServiceUriKey);
            string tags = defaults.StringForKey(TagsKey);
            string runId = defaults.StringForKey(RunIdKey);
            string runtimeVersion = defaults.StringForKey(RuntimeVersionKey);

            this.uriEntry = new AccessibleEntryElement(null, "Mobile Service URI", mobileServiceUri, accessibilityId: MobileServiceUriKey);
            this.tagsEntry = new AccessibleEntryElement(null, "Tags", tags, accessibilityId: TagsKey);

            this.runIdEntry = new AccessibleEntryElement(null, "Run Id", runId, accessibilityId: RunIdKey);
            this.runtimeVersionEntry = new AccessibleEntryElement(null, "Runtime version", runtimeVersion, accessibilityId: RuntimeVersionKey);

            Root = new RootElement(".NET Client Library Tests") {
                new Section("Login") {
                    this.uriEntry,
                    this.tagsEntry
                },

                new Section("Report Results"){
                    this.runIdEntry,
                    this.runtimeVersionEntry
                },

                new Section {
                   new AccessibleStringElement ("Run Tests", RunTests, accessibilityId: "RunTests")
                }
            };
        }

        private const string MobileServiceUriKey = "MobileServiceUri";
        private const string TagsKey = "Tags";
        private const string RunIdKey = "RunId";
        private const string RuntimeVersionKey = "RuntimeVersion";

        private readonly EntryElement uriEntry;
        private readonly EntryElement tagsEntry;
        private readonly EntryElement runIdEntry;
        private readonly EntryElement runtimeVersionEntry;

        private void RunTests()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.SetString(this.uriEntry.Value, MobileServiceUriKey);
            defaults.SetString(this.tagsEntry.Value, TagsKey);
            defaults.SetString(this.runIdEntry.Value, RunIdKey);
            defaults.SetString(this.runtimeVersionEntry.Value, RuntimeVersionKey);

            AppDelegate.Harness.SetAutoConfig(new TestConfig()
            {
                MobileServiceRuntimeUrl = this.uriEntry.Value,
                TagExpression = this.tagsEntry.Value,
                RuntimeVersion = this.runtimeVersionEntry.Value
            });
            AppDelegate.Harness.Settings.ManualMode = string.IsNullOrWhiteSpace(this.runtimeVersionEntry.Value);

            NavigationController.PushViewController(new HarnessViewController(), true);
        }
    }
}