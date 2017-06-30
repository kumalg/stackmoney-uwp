namespace Finanse.Models.Interfaces {
    public interface ISync {
        string LastModifed { get; set; }
        bool IsDeleted { get; set; }
        string GlobalId { get; set; }
    }
}
