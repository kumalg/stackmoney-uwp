using Windows.System.Profile;

namespace Finanse.Models.Helpers {
    public class SystemVersionHelper {
        static string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
        static ulong v = ulong.Parse(deviceFamilyVersion);
        
        public static ulong Major => (v & 0xFFFF000000000000L) >> 48;
        public static ulong Minor => (v & 0x0000FFFF00000000L) >> 32;
        public static ulong Revision => (v & 0x00000000FFFF0000L) >> 16;
        public static ulong Build => v & 0x000000000000FFFFL;
    }
}
