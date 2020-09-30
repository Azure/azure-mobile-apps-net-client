// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Foundation;
using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        public static IPlatformInformation Instance { get; } = new PlatformInformation();

        public string OperatingSystemArchitecture => PlatformID.MacOSX.ToString();

        public string OperatingSystemName => "macOS";

        public string OperatingSystemVersion
        { 
            get
            {
                var osInfo = NSProcessInfo.ProcessInfo.OperatingSystemVersion;
                return $"{osInfo.Major}.{osInfo.Minor}.{osInfo.PatchVersion}";
            }
        }

        public bool IsEmulator => false;

        public string Version => this.GetVersionFromAssemblyFileVersion();
    }
}
