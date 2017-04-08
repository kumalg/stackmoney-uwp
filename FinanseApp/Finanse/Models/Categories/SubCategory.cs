namespace Finanse.Models.Categories {
    public class SubCategory : Category {

        public string BossCategoryId { get; set; }

        public SubCategory(Category category) {
            if (category is SubCategory)
                BossCategoryId = ((SubCategory)category).BossCategoryId;

            Id = category.Id;
            IconKey = category.IconKey;
            ColorKey = category.ColorKey;
            Name = category.Name;
            VisibleInExpenses = category.VisibleInExpenses;
            VisibleInIncomes = category.VisibleInIncomes;
            GlobalId = category.GlobalId;
            CantDelete = category.CantDelete;
            LastModifed = category.LastModifed;
            IsDeleted = category.IsDeleted;
        }


        public new SubCategory Clone() => (SubCategory)MemberwiseClone();

        public SubCategory() {
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (!(o is SubCategory))
                return false;

            return
                base.Equals((Category)o) &&
                ((SubCategory)o).BossCategoryId == BossCategoryId;
        }
    }
}
