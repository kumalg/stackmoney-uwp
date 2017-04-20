using Windows.System.Profile;

namespace Finanse.Models.SystemVersion {
    public class SystemCurrentVersion {
        private static readonly string DeviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
        private static readonly ulong V = ulong.Parse(DeviceFamilyVersion);
        
        public static ulong Major => (V & 0xFFFF000000000000L) >> 48;
        public static ulong Minor => (V & 0x0000FFFF00000000L) >> 32;
        public static ulong Revision => (V & 0x00000000FFFF0000L) >> 16;
        public static ulong Build => V & 0x000000000000FFFFL;
    }
}
