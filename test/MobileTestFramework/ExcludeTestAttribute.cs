// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace MobileTest.Framework
{
    /// <summary>
    /// Mark a test as excluded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExcludeTestAttribute : Attribute
    {
        public string Reason { get; private set; }

        public ExcludeTestAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
