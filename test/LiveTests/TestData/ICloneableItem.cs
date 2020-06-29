// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace MobileTest.LiveTests.TestData
{
    public interface ICloneableItem<T> where T : ICloneableItem<T>
    {
        T Clone();
        object Id { get; set; }
    }
}
