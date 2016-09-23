using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {
    public class StoreData {

        private readonly ItemCollection _collection = new ItemCollection();
        public StoreData() {

        }
        /*
        public ItemCollection Collection {
            get {
                return _collection;
            }
        }
        */
        internal ObservableCollection<GroupInfoList<Operation>> GetGroupsByDay() {

            ObservableCollection<GroupInfoList<Operation>> groups = new ObservableCollection<GroupInfoList<Operation>>();
            
            decimal sumCost = 0;
            DateTimeOffset? dt;

            Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllPersons()
                        group item by String.Format("{0:yyyy/MM/dd}", ((DateTimeOffset)item.Date).LocalDateTime) into g
                        orderby g.Key descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                GroupInfoList<Operation> info = new GroupInfoList<Operation>() {
                    Key = g.GroupName,
                };
                dt = Convert.ToDateTime(g.GroupName);

                info.dayNum = String.Format("{0:dd}", dt);
                info.day = String.Format("{0:dddd}", dt);
                info.month = String.Format("{0:MMMM yyyy}", dt);

                sumCost = 0;

                foreach (var item in g.Items) {
                    info.Add(item);
                    if (item.isExpense)
                        sumCost -= item.Cost;
                    else
                        sumCost += item.Cost;
                }
                info.decimalCost = sumCost;
                info.cost = sumCost.ToString("C", new CultureInfo(settings.CultureInfoName));
                groups.Add(info);
            }

            return groups;
        }
    }
}
