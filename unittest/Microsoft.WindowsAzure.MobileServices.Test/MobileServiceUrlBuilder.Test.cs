// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [TestClass]
    public class MobileServiceUrlBuilder_Test
    {
        [TestMethod]
        public void GetQueryString_Basic()
        {
            var parameters = new Dictionary<string, string>() { { "x", "$y" }, { "&hello", "?good bye" }, { "a$", "b" } };
            Assert.AreEqual("x=%24y&%26hello=%3Fgood%20bye&a%24=b", MobileServiceUrlBuilder.GetQueryString(parameters));
        }

        [TestMethod]
        public void GetQueryString_Null()
        {
            Assert.AreEqual(null, MobileServiceUrlBuilder.GetQueryString(null));
        }

        [TestMethod]
        public void GetQueryString_Empty()
        {
            Assert.AreEqual(null, MobileServiceUrlBuilder.GetQueryString(new Dictionary<string, string>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetQueryString_Invalid()
        {
            var parameters = new Dictionary<string, string>() { { "$x", "someValue" } };
            MobileServiceUrlBuilder.GetQueryString(parameters);
        }

        [TestMethod]
        public void CombinePathAndQueryTest()
        {
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", "x=y&a=b"));
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", "?x=y&a=b"));
            Assert.AreEqual("somePath", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", null));
            Assert.AreEqual("somePath", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", ""));
        }

        [TestMethod]
        public void AddTrailingSlashTest()
        {
            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc"), "http://abc/");
            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc/"), "http://abc/");

            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc/def"), "http://abc/def/");
            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc/def/"), "http://abc/def/");

            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc/     "), "http://abc/     /");
            Assert.AreEqual(MobileServiceUrlBuilder.AddTrailingSlash("http://abc/def/     "), "http://abc/def/     /");
        }
    }
}

