// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public interface ICloneableItem<T> where T : ICloneableItem<T>
    {
        T Clone();
        object Id { get; set; }
    }
}
