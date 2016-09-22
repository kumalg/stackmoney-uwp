using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {

    public class OperationPattern {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Title { get; set; } 
        public int CategoryId { get; set; } 
        public int SubCategoryId { get; set; } 
        public decimal Cost { get; set; } 
        public bool isExpense { get; set; } 
        public string PayForm { get; set; }
        public int MoneyAccountId { get; set; }
        public string MoreInfo { get; set; }
    }
}
