using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Finanse.DataAccessLayer;
using Finanse.Models.Categories;
using Finanse.Models.Operations;

namespace FinanseUnitTests {
    [TestClass]
    public class OperationTests {
        private SubCategory subCategory = new SubCategory {
            Name = "TestSubCategory",
            BossCategoryId = 1
        };

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

        [TestMethod]
        public void ExistCategoryWithName() {
            DalBase.CreateDb();
            Assert.IsTrue(Dal.CategoryExistByName("Inne"));
        }


        [TestMethod]
        public void NotExistCategoryWithName() {
            DalBase.CreateDb();
            Assert.IsFalse(Dal.CategoryExistByName("Inne2"));
        }

        [TestMethod]
        public void ExistSubCategoryWithName() {
            DalBase.CreateDb();
            Dal.AddSubCategory(subCategory);
            Assert.IsTrue(Dal.SubCategoryExistInBaseByName(subCategory.Name, subCategory.BossCategoryId));
        }
        
        [TestMethod]
        public void NotExistSubCategoryWithName() {
            DalBase.CreateDb();
            Dal.AddSubCategory(subCategory);
            Assert.IsFalse(Dal.SubCategoryExistInBaseByName(subCategory.Name + "NotExist", subCategory.BossCategoryId));
        }
    }
}
