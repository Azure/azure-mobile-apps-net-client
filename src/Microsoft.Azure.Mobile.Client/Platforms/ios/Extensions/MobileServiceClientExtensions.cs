//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension methods for UI-based login.
    /// </summary>
    public static partial class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider, string uriScheme)
        {
            return LoginAsync(client, viewController, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default, viewController, provider.ToString(), uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, string provider, string uriScheme)
        {
            return LoginAsync(client, viewController, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, string provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default, viewController, provider, uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider, string uriScheme)
        {
            return LoginAsync(client, rectangle, view, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, rectangle, (object)view, provider.ToString(), uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, string provider, string uriScheme)
        {
            return LoginAsync(client, rectangle, view, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, string provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, rectangle, (object)view, provider, uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIBarButtonItem barButtonItem, MobileServiceAuthenticationProvider provider, string uriScheme)
        {
            return LoginAsync(client, barButtonItem, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIBarButtonItem barButtonItem, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default, barButtonItem, provider.ToString(), uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIBarButtonItem barButtonItem, string provider, string uriScheme)
        {
            return LoginAsync(client, barButtonItem, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.MobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The URL scheme of the application.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIBarButtonItem barButtonItem, string provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default, barButtonItem, provider, uriScheme, parameters);
        }

        internal static Task<MobileServiceUser> LoginAsync(MobileServiceClient client, RectangleF rect, object view, string provider, string uriScheme, IDictionary<string, string> parameters)
        {
            var auth = new MobileServiceUIAuthentication(rect, view, client, provider, uriScheme, parameters);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Resume login process with the specified URL.
        /// </summary>
        public static bool ResumeWithURL(this MobileServiceClient client, NSUrl url)
        {
            // Convert iOS NSUrl to C#/netxf/BCL System.Uri - common API
            Uri uri_netfx = new Uri(url.AbsoluteString);

            MobileServiceUIAuthentication.CurrentAuthenticator?.OnPageLoading(uri_netfx);

            return true;
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to create with.
        /// </param>
        /// <returns>
        /// The <see cref="Push"/> object used for registering for notifications.
        /// </returns>
        public static Push GetPush(this IMobileServiceClient client)
        {
            return new Push(client);
        }
    }
}
