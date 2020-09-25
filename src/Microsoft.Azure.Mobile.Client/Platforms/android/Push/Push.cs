//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Define a class help to create/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private IMobileServiceClient Client { get; set; }

        internal Push(IMobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (!(client is MobileServiceClient internalClient))
            {
                throw new ArgumentException("Client must be a MobileServiceClient object", nameof(client));
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
        /// Register an Installation with particular registrationId
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId)
        {
            return this.RegisterAsync(registrationId, null);
        }

        /// <summary>
        /// Register an Installation with particular registrationId and templates
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId, JObject templates)
        {
            if (string.IsNullOrWhiteSpace(registrationId))
            {
                throw new ArgumentNullException(nameof(registrationId));
            }

            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = registrationId;
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
    }
}
