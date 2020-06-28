// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace Microsoft.WindowsAzure.MobileServices.Test.Eventing
{
    public class TestEvent<T> : IMobileServiceEvent
    {
        public TestEvent(T payload)
        {
            this.Payload = payload;
        }

        public string Name
        {
            get { return "TestEvent"; }
        }

        public T Payload { get; set; }
    }

    [TestClass]
    public class MobileServiceEventManager_Test
    {
        [TestMethod]
        public void Subscribe_FiltersOnGenericType()
        {
            var eventManager = new MobileServiceEventManager();

            var testEvent = new TestEvent<bool>(false);
            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);

            IDisposable testEventSubscription = eventManager.Subscribe<TestEvent<bool>>(e => e.Payload = true);

            eventManager.PublishAsync(testEvent).Wait(1000);
            eventManager.PublishAsync(mobileServiceEvent).Wait(1000);

            Assert.IsTrue(testEvent.Payload, "Test event was not handled");
            Assert.IsFalse(mobileServiceEvent.Payload, "Mobile service event was not filtered");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_OnSubscriptionHandler_Succeeds()
        {
            var eventManager = new MobileServiceEventManager();

            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);
            bool eventHandled = false;
            IDisposable innerSubscription = null;
            IDisposable outerSubscription = eventManager
                .Subscribe<IMobileServiceEvent>(e => innerSubscription = eventManager.Subscribe<IMobileServiceEvent>(b => eventHandled = true));


            bool result = eventManager.PublishAsync(mobileServiceEvent).Wait(1000);
            Assert.IsTrue(result, "Subscribe failed");

            outerSubscription.Dispose();

            eventManager.PublishAsync(mobileServiceEvent).Wait(1000);
            Assert.IsTrue(eventHandled, "Subscribe failed");
        }

        [TestMethod]
        public async Task Subscribe_FiltersOnObserverGenericType()
        {
            var eventManager = new MobileServiceEventManager();

            var testEvent = new TestEvent<bool>(false);
            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);

            var mre = new ManualResetEventSlim();

            var observer = new MobileServiceEventObserver<TestEvent<bool>>(e => {
                e.Payload = true;
                mre.Set();
            });

            IDisposable testEventSubscription = eventManager.Subscribe(observer);

            await eventManager.PublishAsync(testEvent);
            bool eventSet = mre.Wait(500);
            Assert.IsTrue(eventSet);
            Assert.IsTrue(testEvent.Payload, "Test event was not handled");

            mre.Reset();
            await eventManager.PublishAsync(mobileServiceEvent);
            eventSet = mre.Wait(500);
            Assert.IsFalse(eventSet);
            Assert.IsFalse(mobileServiceEvent.Payload);

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_WithObserver_DoesNotFilterDerivedTypes()
        {
            var eventManager = new MobileServiceEventManager();

            var testEventA = new DerivedEventA<bool>(false);
            var testEventB = new DerivedEventA<bool>(false);

            var observer = new MobileServiceEventObserver<TestEvent<bool>>(e => e.Payload = true);

            IDisposable testEventSubscription = eventManager.Subscribe(observer);

            eventManager.PublishAsync(testEventA).Wait(1000);
            eventManager.PublishAsync(testEventB).Wait(1000);

            Assert.IsTrue(testEventA.Payload, "Derived event A was not handled");
            Assert.IsTrue(testEventA.Payload, "Derived event B was not handled");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_WithGenericType_DoesNotFilterDerivedTypes()
        {
            var eventManager = new MobileServiceEventManager();

            var testEventA = new DerivedEventA<bool>(false);
            var testEventB = new DerivedEventA<bool>(false);

            IDisposable testEventSubscription = eventManager.Subscribe<TestEvent<bool>>(e => e.Payload = true);

            eventManager.PublishAsync(testEventA).Wait(1000);
            eventManager.PublishAsync(testEventB).Wait(1000);

            Assert.IsTrue(testEventA.Payload, "Derived event A was not handled");
            Assert.IsTrue(testEventA.Payload, "Derived event B was not handled");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_Throws_WhenObserverIsNull()
        {
            var eventManager = new MobileServiceEventManager();
            IObserver<IMobileServiceEvent> observer = null;
            eventManager.Subscribe(observer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_Throws_WhenActionIsNull()
        {
            var eventManager = new MobileServiceEventManager();
            Action<IMobileServiceEvent> action = null;
            eventManager.Subscribe(action);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PublishAsync_Throws_WhenEventIsNull()
        {
            var eventManager = new MobileServiceEventManager();
            await eventManager.PublishAsync(null);
        }

        private class DerivedEventA<T> : TestEvent<T>
        {
            public DerivedEventA(T payload)
                : base(payload)
            { }
        }

        private class DerivedEventB<T> : TestEvent<T>
        {
            public DerivedEventB(T payload)
                : base(payload)
            { }
        }
    }
}
