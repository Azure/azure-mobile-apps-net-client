//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;
using NSAction = System.Action;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceUIAuthentication : MobileServicePKCEAuthentication
    {
        private readonly RectangleF rect;
        private readonly object view;

        public MobileServiceUIAuthentication(RectangleF rect, object view, MobileServiceClient client, string providerName, string uriScheme, IDictionary<string, string> parameters)
            : base(client, providerName, uriScheme, parameters)
        {
            this.rect = rect;
            this.view = view;
        }

        internal static WebAuthenticator CurrentAuthenticator;

        protected override Task<string> GetAuthorizationCodeAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            if (CurrentAuthenticator != null)
            {
                tcs.TrySetException(new InvalidOperationException("Authentication is already in progress."));
                return tcs.Task;
            }

            CurrentAuthenticator = new WebRedirectAuthenticator(LoginUri, CallbackUri)
            {
                ClearCookiesBeforeLogin = false
            };

            UIViewController c = CurrentAuthenticator.GetUI();

            UIViewController controller = null;
            UIPopoverController popover = null;

            CurrentAuthenticator.Error += (o, e) =>
            {
                void completed()
                {
                    Exception ex = e.Exception ?? new Exception(e.Message);
                    tcs.TrySetException(ex);
                }

                if (controller != null)
                {
                    controller.DismissViewController(true, null);
                    completed();
                }

                if (popover != null)
                {
                    popover.Dismiss(true);
                    completed();
                }
                CurrentAuthenticator = null;
            };

            CurrentAuthenticator.Completed += (o, e) =>
            {
                void completed()
                {
                    if (!e.IsAuthenticated)
                    {
                        tcs.TrySetException(new InvalidOperationException("Authentication was cancelled by the user."));
                    }
                    else if (!e.Account.Properties.TryGetValue("authorization_code", out string authorizationCode))
                    {
                        tcs.TrySetException(new InvalidOperationException("Authentication failed: could not found \"authorization_code\"."));
                    }
                    else
                    {
                        tcs.TrySetResult(authorizationCode);
                    }
                }

                if (controller != null)
                {
                    controller.DismissViewController(true, null);
                    completed();
                }

                if (popover != null)
                {
                    popover.Dismiss(true);
                    completed();
                }
                CurrentAuthenticator = null;
            };

            controller = view as UIViewController;
            if (controller != null)
            {
                controller.PresentViewController(c, true, null);
            }
            else
            {
                UIView v = view as UIView;

                popover = new UIPopoverController(c);

                if (view is UIBarButtonItem barButton)
                {
                    popover.PresentFromBarButtonItem(barButton, UIPopoverArrowDirection.Any, true);
                }
                else
                {
                    popover.PresentFromRect(rect, v, UIPopoverArrowDirection.Any, true);
                }
            }

            return tcs.Task;
        }
    }
}