// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [TestClass]
    public class MobileServiceUser_Test
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullThrowsException()
        {
            var test = new MobileServiceUser(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_EmptyThrowsException()
        {
            var test = new MobileServiceUser(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WhitespaceThrowsException()
        {
            var test = new MobileServiceUser(" ");
        }

        [TestMethod]
        public void Constructor_BuildsObject()
        {
            var test = new MobileServiceUser("userId");
            Assert.AreEqual(test.UserId, "userId");
        }
    }
}