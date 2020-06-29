// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace MobileTest.Framework
{
    /// <summary>
    /// Declare an async test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AsyncTestMethodAttribute : Attribute
    {
    }
}
