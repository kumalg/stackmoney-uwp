namespace Finanse.Models {
    public class HeaderItem {
        public string Day { get; set; }
        public bool IsEnabled { get; set; }
    }
    public class Operation : OperationPattern {

        public string Date { get; set; }
        public bool VisibleInStatistics { get; set; } = true;

        public override int GetHashCode() {
            return Title.GetHashCode() * this.Id;
        }
        public override bool Equals(object o) {
            if (!(o is Operation))
                return false;

            return
                base.Equals((OperationPattern)o) &&
                ((Operation)o).Date == Date &&
                ((Operation)o).VisibleInStatistics == VisibleInStatistics;
        }
    }
}
