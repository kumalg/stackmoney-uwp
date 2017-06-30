using System;
using Finanse.Models.Extensions.DateTimeExtensions;
using Finanse.Models.Helpers;

namespace Finanse.Models.Extensions {
    public static class SyncExtensions {
        public static string NewGlobalIdFromLocal(this int id) {
            return $"{Informations.DeviceId}_{id}_{DateTime.UtcNow.GetTimestamp()}";
        }
    }
}
