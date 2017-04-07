using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanse.Models.Helpers;

namespace Finanse.Models.DateTimeExtensions {
    public static class SyncExtensions {
        public static string NewGlobalIdFromLocal(this int id) {
            return $"{Informations.DeviceId}_{id}_{DateTime.UtcNow.GetTimestamp()}";
        }
    }
}
