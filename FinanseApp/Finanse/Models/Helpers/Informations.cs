using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace Finanse.Models.Helpers {
    public class Informations {
        public static string DeviceId {
            get {
                var deviceInformation = new EasClientDeviceInformation();
                return deviceInformation.Id.ToString();
            }
        }

        public static string DisplayName => Package.Current.DisplayName;

        public static string PublisherDisplayName => Package.Current.PublisherDisplayName;

        public static string AppVersion {
            get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
    }
}
