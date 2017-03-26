namespace Finanse.Models {
    public abstract class SyncProperties {
        public string LastModifed { get; set; }
        public string DeviceId { get; set; }
        public int RemoteId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
