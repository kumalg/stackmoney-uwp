using SQLite.Net.Attributes;

namespace Finanse.Models {
    public class Deleted {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Index { get; set; }
        public string TableName { get; set; }
    }
}
