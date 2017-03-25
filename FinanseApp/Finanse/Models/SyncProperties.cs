namespace Finanse.Models {
    public class SyncProperties {
        public string LastModifed { get; set; }
        public string DeviceId { get; set; } //ale prawdopodobie tylko get
        public int RemoteId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
