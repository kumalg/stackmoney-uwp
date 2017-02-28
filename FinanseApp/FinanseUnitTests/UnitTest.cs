using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Finanse.Models;

namespace FinanseUnitTests {
    [TestClass]
    public class OperationTests {
        [TestMethod]
        public void TestMethod1() {
            var operation = new Operation {
                Title = "dupa"
            };
            Assert.AreEqual(operation.Title, "dupa");
            Assert.AreEqual(operation.VisibleInStatistics, true);
            Assert.AreEqual(operation.isExpense, false);
        }

        [TestMethod]
        public void GroupHeaderByDayTest() {
            var groupHeaderByDay = new GroupHeaderByDay("2016.01.02");
            Assert.AreEqual(groupHeaderByDay.DateTime, new DateTime(2016, 1, 2));
        }
    }
}
