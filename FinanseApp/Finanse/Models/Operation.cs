namespace Finanse.Models {
    public class HeaderItem {
        public string Day { get; set; }
        public bool IsEnabled { get; set; }
    }
    public class Operation : OperationPattern {

        public string Date { get; set; }
        public bool VisibleInStatistics { get; set; }

        public override int GetHashCode() {
            return this.Title.GetHashCode() * this.Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is Operation))
                return false;

            return
                base.Equals((OperationPattern)o) &&
                ((Operation)o).Date == Date &&
                ((Operation)o).VisibleInStatistics == VisibleInStatistics;
        }
    }
}
