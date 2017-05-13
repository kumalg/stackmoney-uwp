using System;
using Finanse.Models.Helpers;

namespace Finanse.Models.DateTimeExtensions {
    public static class SyncExtensions {
        public static string NewGlobalIdFromLocal(this int id) {
            return $"{Informations.DeviceId}_{id}_{DateTime.UtcNow.GetTimestamp()}";
        }
    }
}
