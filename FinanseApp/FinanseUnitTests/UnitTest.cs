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
            BossCategoryId = "1"
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
            Assert.IsTrue(CategoriesDal.CategoryExistInBaseByName("Inne"));
        }


        [TestMethod]
        public void NotExistCategoryWithName() {
            DalBase.CreateDb();
            Assert.IsFalse(CategoriesDal.CategoryExistInBaseByName("Inne2"));
        }

        [TestMethod]
        public void ExistSubCategoryWithName() {
            DalBase.CreateDb();
            CategoriesDal.AddCategory(subCategory);
            Assert.IsTrue(CategoriesDal.CategoryExistInBaseByName(subCategory.Name));
        }
        
        [TestMethod]
        public void NotExistSubCategoryWithName() {
            DalBase.CreateDb();
            CategoriesDal.AddCategory(subCategory);
            Assert.IsFalse(CategoriesDal.CategoryExistInBaseByName(subCategory.Name + "NotExist"));
        }
    }
}
