// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Provides access to platform-specific framework API's.
    /// </summary>
    public static class TestPlatform
    {
        public static readonly string Net45 = "Net45";
        public static readonly string UniversalWindowsPlatform = "UniversalWindowsPlatform";
        public static readonly string XamarinAndroid = "XamarinAndroid";
        public static readonly string XamariniOS = "XamariniOS";

        public static string GetMobileServicesSdkVersion(Assembly executingAssembly)
        {
            string packagesConfigResourceName = executingAssembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(".packages.config"));
            if (packagesConfigResourceName == null)
                return "?";

            using (Stream stream = executingAssembly.GetManifestResourceStream(packagesConfigResourceName))
            {
                StreamReader reader = new StreamReader(stream);
                string result = reader.ReadToEnd();
                string line = result.Split('\n').FirstOrDefault(s => s.Contains("\"WindowsAzure.MobileServices\""));
                if (line == null)
                    return "?";
                const string versionSearchString = "version=\"";
                int index = line.IndexOf(versionSearchString);
                if (index < 0 || index + versionSearchString.Length >= line.Length)
                    return "?";
                int closeIndex = line.IndexOf('"', index + versionSearchString.Length);
                if (closeIndex < 0)
                    return "?";

                return line.Substring(index + versionSearchString.Length, closeIndex - index - versionSearchString.Length);
            }
        }

        /// <summary>
        /// The string value to use for the operating system name, arch, or version if
        /// the value is unknown.
        /// </summary>
        public const string UnknownValueString = "--";

        private static ITestPlatform current;

        /// <summary>
        /// Name of the assembly containing the Class with the <see cref="Platform.PlatformTypeFullName"/> name.
        /// </summary>
        public static IList<string> PlatformAssemblyNames = new string[]
        {
            "Android.E2ETest",
            "MicrosoftWindowsAzureMobileiOSE2ETest"
        };

        /// <summary>
        /// Name of the type implementing <see cref="IPlatform"/>.
        /// </summary>
        public static string PlatformTypeFullName = "Microsoft.WindowsAzure.MobileServices.Test.CurrentTestPlatform";

        /// <summary>
        /// Gets the current platform. If none is loaded yet, accessing this property triggers platform resolution.
        /// </summary>
        public static ITestPlatform Instance
        {
            get
            {
                // create if not yet created
                if (current == null)
                {
                    // assume the platform assembly has the same key, same version and same culture
                    // as the assembly where the ITestPlatform interface lives.
                    var provider = typeof(ITestPlatform);
                    var asm = new AssemblyName(provider.GetTypeInfo().Assembly.FullName);

                    // change name to the specified name
                    foreach (string assemblyName in PlatformAssemblyNames)
                    {
                        asm.Name = assemblyName;
                        var name = PlatformTypeFullName + ", " + asm.FullName;

                        //look for the type information but do not throw if not found
                        var type = Type.GetType(name, false);

                        if (type != null)
                        {
                            // create type
                            // since we are the only one implementing this interface
                            // this cast is safe.
                            current = (ITestPlatform)Activator.CreateInstance(type);
                            return current;
                        }
                    }

                    current = new MissingTestPlatform();
                }

                return current;
            }

            // keep this public so we can set a TestPlatform for unit testing.
            set
            {
                current = value;
            }
        }
    }
}