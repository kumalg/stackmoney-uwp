using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {
    class MoneyAccount {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name { get; set; } 
    }
}
