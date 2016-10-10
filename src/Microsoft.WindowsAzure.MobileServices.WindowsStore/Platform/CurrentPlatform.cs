// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        public IApplicationStorage ApplicationStorage { get { return Microsoft.WindowsAzure.MobileServices.ApplicationStorage.Instance; } }

        /// <summary>
        /// Returns a platform-specific implemention of platform information.
        /// </summary>
        public IPlatformInformation PlatformInformation { get { return Microsoft.WindowsAzure.MobileServices.PlatformInformation.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for manipulating
        /// <see cref="System.Linq.Expressions.Expression"/> instances.
        /// </summary>
        public IExpressionUtility ExpressionUtility { get { return Microsoft.WindowsAzure.MobileServices.ExpressionUtility.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for platform-specifc push capabilities.
        /// </summary>
        public IPushUtility PushUtility { get { return Microsoft.WindowsAzure.MobileServices.PushUtility.Instance; } }

        /// <summary>
        /// Returns a platform-specific path for storing offline databases
        /// that are not fully-qualified.
        /// </summary>
        public string DefaultDatabasePath
        {
            get
            {
                return ApplicationData.Current.LocalFolder.Path;
            }
        }

        /// <summary>
        /// Retrieves an ApplicationStorage where all items stored are segmented from other stored items
        /// </summary>
        /// <param name="name">The name of the segemented area in application storage</param>
        /// <returns>The specific instance of that segment</returns>
        public IApplicationStorage GetNamedApplicationStorage(string name)
        {
            return new ApplicationStorage(name);
        }

        /// <summary>
        /// Ensures that a file exists, creating it if necessary
        /// </summary>
        /// <param name="path">The fully-qualified pathname to check</param>
        public void EnsureFileExists(string path)
        {
            var folder = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);

            // This section is executed synchronously.
            EnsureFileExistsAsync(folder, file).RunSynchronously();
            // End of synchronous section
        }

        async Task EnsureFileExistsAsync(string folder, string file)
        {
            StorageFolder folderRef = await StorageFolder.GetFolderFromPathAsync(folder);
            await folderRef.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
        }
    }
}
