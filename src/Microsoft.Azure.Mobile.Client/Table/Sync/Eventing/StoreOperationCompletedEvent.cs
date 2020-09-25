// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Represents an event raised when an operation against the local store completes.
    /// </summary>
    public sealed class StoreOperationCompletedEvent : StoreChangeEvent
    {
        /// <summary>
        /// The store operation completed event name.
        /// </summary>
        public const string EventName = "MobileServices.StoreOperationCompleted";

        public StoreOperationCompletedEvent(StoreOperation operation)
        {
            Arguments.IsNotNull(operation, nameof(operation));
            Operation = operation;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public override string Name
        {
            get { return EventName; }
        }

        /// <summary>
        /// The operation that triggered this event.
        /// </summary>
        public StoreOperation Operation { get; private set; }
    }
}
