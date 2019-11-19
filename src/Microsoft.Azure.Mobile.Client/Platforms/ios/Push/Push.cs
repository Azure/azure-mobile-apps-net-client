//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text;
    using Newtonsoft.Json.Linq;

#if __UNIFIED__
    using Foundation;
#else
    using MonoTouch.Foundation;
#endif

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private IMobileServiceClient Client { get; set; }

        internal Push(IMobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            MobileServiceClient internalClient = client as MobileServiceClient;
            if (internalClient == null)
            {
                throw new ArgumentException("Client must be a MobileServiceClient object");
            }

            this.PushHttpClient = new PushHttpClient(internalClient);
            this.Client = client;
        }

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId
        {
            get
            {
                return this.Client.InstallationId;
            }
        }

        /// <summary>
        /// Register an Installation with particular deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken)
        {
            return this.RegisterAsync(deviceToken, null);
        }

        /// <summary>
        /// Register an Installation with particular deviceToken and templates
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken, JObject templates)
        {
            string channelUri = ParseDeviceToken(deviceToken);
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = channelUri;
            installation[PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform();
            if (templates != null)
            {
                JObject templatesWithStringBody = templates;
                foreach (JProperty template in templates.Properties())
                {
                    //Notification hub requires template body to be a string.Convert to string from JObject
                    var templateBody = template.Value["body"];
                    if (templateBody != null && templateBody.GetType() == typeof(JObject))
                    {
                        templatesWithStringBody[template.Name]["body"] = templateBody.ToString();
                    }
                }
                installation[PushInstallationProperties.TEMPLATES] = templatesWithStringBody;
            }
            return this.PushHttpClient.CreateOrUpdateInstallationAsync(installation);
        }

        /// <summary>
        /// Unregister any installations for a particular app
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAsync()
        {
            return this.PushHttpClient.DeleteInstallationAsync();
        }

        internal static string ParseDeviceToken(NSData deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException("deviceToken");
            }
            byte[] byteArray = deviceToken.ToArray();
            if (byteArray.Length == 0)
            {
                throw new ArgumentException("deviceToken.ToArray() returned empty array.");
            }
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}