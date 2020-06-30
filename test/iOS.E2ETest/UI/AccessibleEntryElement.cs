using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.Dialog;
using Foundation;
using UIKit;

namespace Microsoft.WindowsAzure.Mobile.iOS.Test
{
    class AccessibleEntryElement : EntryElement
    {
        public AccessibleEntryElement(string caption, string placeholder, string value, string accessibilityId) :
            base(caption, placeholder, value)
        {
            this.AccessibilityId = accessibilityId;
        }

        public AccessibleEntryElement(string caption, string placeholder, string value, bool isPassword, string accessibilityId) :
            base(caption, placeholder, value, isPassword)
        {
            this.AccessibilityId = accessibilityId;
        }

        public string AccessibilityId { get; private set; }


        protected override UITextField CreateTextField(CoreGraphics.CGRect frame)
        {
            var result = base.CreateTextField(frame);
            result.AccessibilityIdentifier  = this.AccessibilityId;
            return result;
        }
    }
}