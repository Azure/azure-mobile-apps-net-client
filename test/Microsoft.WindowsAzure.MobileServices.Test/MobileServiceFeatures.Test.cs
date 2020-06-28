// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [TestClass]
    public class MobileServiceFeatures_Test
    {
        [TestMethod]
        public void Validate_TT_FeatureCode()
        {
            Assert.AreEqual("TT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedTable));
        }

        [TestMethod]
        public void Validate_TU_FeatureCode()
        {
            Assert.AreEqual("TU", EnumValueAttribute.GetValue(MobileServiceFeatures.UntypedTable));
        }

        [TestMethod]
        public void Validate_AT_FeatureCode()
        {
            Assert.AreEqual("AT", EnumValueAttribute.GetValue(MobileServiceFeatures.TypedApiCall));
        }

        [TestMethod]
        public void Validate_AJ_FeatureCode()
        {
            Assert.AreEqual("AJ", EnumValueAttribute.GetValue(MobileServiceFeatures.JsonApiCall));
        }

        [TestMethod]
        public void Validate_AG_FeatureCode()
        {
            Assert.AreEqual("AG", EnumValueAttribute.GetValue(MobileServiceFeatures.GenericApiCall));
        }

        [TestMethod]
        public void Validate_TC_FeatureCode()
        {
            Assert.AreEqual("TC", EnumValueAttribute.GetValue(MobileServiceFeatures.TableCollection));
        }

        [TestMethod]
        public void Validate_OL_FeatureCode()
        {
            Assert.AreEqual("OL", EnumValueAttribute.GetValue(MobileServiceFeatures.Offline));
        }

        [TestMethod]
        public void Validate_QS_FeatureCode()
        {
            Assert.AreEqual("QS", EnumValueAttribute.GetValue(MobileServiceFeatures.AdditionalQueryParameters));
        }

        [TestMethod]
        public void Validate_RT_FeatureCode()
        {
            Assert.AreEqual("RT", EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken));
        }

        [TestMethod]
        public void Validate_FeatureCode_None()
        {
            Assert.IsNull(EnumValueAttribute.GetValue(MobileServiceFeatures.None));
        }
    }
}
