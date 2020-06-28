// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Test.OData
{
    [TestClass]
    public class ODataExpressionParser_Test
    {
        class CurrentCultureHelper : IDisposable
        {
            private CultureInfo previous;

            public static CultureInfo CurrentCulture
            {
                get => CultureInfo.CurrentCulture;
                set => CultureInfo.CurrentCulture = value;
            }

            public CurrentCultureHelper(string name)
            {
                previous = CurrentCulture;
                CurrentCulture = new CultureInfo(name);
            }

            public void Dispose()
            {
                CurrentCulture = previous;
            }
        }

        [TestMethod]
        public void ParseFilter_Real_NumberDecimalSeparator()
        {
            // Set some CultureInfo with different decimal separator
            using (var _ = new CurrentCultureHelper("ru-RU"))
            {
                QueryNode queryNode = ODataExpressionParser.ParseFilter("Field eq 42.42");

                Assert.IsNotNull(queryNode);

                var comparisonNode = queryNode as BinaryOperatorNode;
                Assert.IsNotNull(comparisonNode);

                var left = comparisonNode.LeftOperand as MemberAccessNode;
                Assert.IsNotNull(left);

                var right = comparisonNode.RightOperand as ConstantNode;
                Assert.IsNotNull(right);

                Assert.AreEqual("Field", left.MemberName);
                Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
                Assert.AreEqual(42.42, right.Value);
            }
        }

        [TestMethod]
        public void ParseFilter_Guid()
        {
            Guid filterGuid = Guid.NewGuid();

            QueryNode queryNode = ODataExpressionParser.ParseFilter(string.Format("Field eq guid'{0}'", filterGuid));

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(right);

            Assert.AreEqual("Field", left.MemberName);
            Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.AreEqual(filterGuid, right.Value);
        }

        [TestMethod]
        public void ParseFilter_TrueToken()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("(true eq null) and false");

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as BinaryOperatorNode;
            Assert.IsNotNull(left);

            var trueNode = left.LeftOperand as ConstantNode;
            Assert.IsNotNull(trueNode);
            Assert.AreEqual(true, trueNode.Value);

            var nullNode = left.RightOperand as ConstantNode;
            Assert.IsNotNull(nullNode);
            Assert.AreEqual(null, nullNode.Value);

            var falseNode = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(falseNode);

            Assert.AreEqual(BinaryOperatorKind.And, comparisonNode.OperatorKind);
            Assert.AreEqual(false, falseNode.Value);
        }

        [TestMethod]
        public void ParseFilter_DateTimeMember()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("datetime eq 1");

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(right);

            Assert.AreEqual("datetime", left.MemberName);
            Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.AreEqual(1L, right.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(MobileServiceODataException))]
        public void ParseFilter_Guid_InvalidGuidString()
        {
            ODataExpressionParser.ParseFilter(string.Format("Field eq guid'this is not a guid'"));
        }
    }
}
