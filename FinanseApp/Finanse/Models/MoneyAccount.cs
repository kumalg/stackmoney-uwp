using SQLite.Net.Attributes;

namespace Finanse.Models {
    class MoneyAccount {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Color { get; set; } 
    }
}
