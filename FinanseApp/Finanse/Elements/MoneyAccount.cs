using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {
    class MoneyAccount {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Color { get; set; } 
    }
}
