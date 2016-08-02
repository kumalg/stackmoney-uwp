using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class Operation {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Title { get; set; } 
        public int CategoryId { get; set; } 
        public int SubCategoryId { get; set; } 
        public decimal Cost { get; set; } 
        public DateTimeOffset? Date { get; set; } 
        public string ExpenseOrIncome { get; set; } 
        public string PayForm { get; set; } 
    }
}
