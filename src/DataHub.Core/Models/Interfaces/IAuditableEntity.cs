namespace DataHub.Core.Models.Interfaces
{
    public interface IAuditableEntity
    {
        public int Id { get; set; }
        DateTime ServerTimestamp { get; set; }
        int ChangeCounter { get; set; }        
    }
}