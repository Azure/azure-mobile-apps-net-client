// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// UI model for a test group.
    /// </summary>
    class GroupDescription : ObservableCollection<TestDescription>
    {
        public string Name { get; set; }

        public GroupDescription()
        {
        }
    }
}
