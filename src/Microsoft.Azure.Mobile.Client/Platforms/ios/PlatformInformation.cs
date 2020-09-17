// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        private static readonly IPlatformInformation instance = new PlatformInformation();

        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        public static IPlatformInformation Instance
        {
            get
            {
                return instance;
            }
        }

        public string OperatingSystemArchitecture
        {
            get { return PlatformID.MacOSX.ToString(); }
        }

        public string OperatingSystemName
        {
            get { return "iOS"; }
        }

        public string OperatingSystemVersion
        {
            get { return UIDevice.CurrentDevice.SystemVersion; }
        }

        public bool IsEmulator
        {
            get
            {
                return (UIDevice.CurrentDevice.Model.ToLower().Contains("simulator"));
            }
        }

        public string Version
        {
            get
            {
                return this.GetVersionFromAssemblyFileVersion();
            }
        }
    }
}
