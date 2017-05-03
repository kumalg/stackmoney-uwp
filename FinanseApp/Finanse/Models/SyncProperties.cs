using System;
using Finanse.DataAccessLayer;
using Finanse.Models.Helpers;

namespace Finanse.Models {
    public abstract class SyncProperties {
        public string LastModifed { get; set; }
        public bool IsDeleted { get; set; }
        public string GlobalId { get; set; }

        public static string GetGlobalId(Type type) => $"{Informations.DeviceId}_{Dal.GetMaxRowId(type) + 1}";
    }
}
