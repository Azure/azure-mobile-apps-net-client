// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test.Table.Serialization
{
    [TestClass]
    public class MobileServiceconverter_Test
    {
        [TestMethod]
        public void CanConvertReturnsTrueForDateTimeDateTimeOffset()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            bool canConvert = converter.CanConvert(typeof(DateTime));
            Assert.IsTrue(canConvert);

            canConvert = converter.CanConvert(typeof(DateTimeOffset));
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvertReturnsFalseForNotDateTimeDateTimeOffset()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            //false
            bool canConvert = converter.CanConvert(typeof(byte));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(ulong));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(int));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(short));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(byte[]));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(object));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(string));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(bool));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(decimal));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(double));
            Assert.IsFalse(canConvert);

            canConvert = converter.CanConvert(typeof(long));
            Assert.IsFalse(canConvert);
        }

        [TestMethod]
        public void CanReadShouldReturnTrue()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            bool canRead = converter.CanRead;

            Assert.IsTrue(canRead);
        }

        [TestMethod]
        public void CanWriteShouldReturnTrue()
        {
            var converter = new MobileServiceIsoDateTimeConverter();

            bool canWrite = converter.CanWrite;

            Assert.IsTrue(canWrite);
        }
    }
}
