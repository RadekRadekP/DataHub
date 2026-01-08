using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RPK_BlazorApp.Models.Interfaces; // Přidáno pro IAuditableEntity

namespace RPK_BlazorApp.Models
{
    public class Operational : IAuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Internal, auto-generated ID

        [Column(Order = 1)]
        [MaxLength(50)]
        public string ClientId { get; set; } = string.Empty;

        [Column(Order = 2)]
        public int ClientDbId { get; set; }  = 0;// ID from the client

        // --- Add the missing properties ---
        [Required] // Assuming Date is always required
        public DateTime Date { get; set; }

        [Required] // Assuming Event ID is always required
        [Column("EventId")] // Explicit column name if desired (optional)
        public int EventId { get; set; }  = 0;// Or string if it's not numeric
        // ---------------------------------

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }  = string.Empty;

        [MaxLength(50)]
        public string? Object { get; set; }  = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Operator { get; set; } = string.Empty;

        public DateTime ServerTimestamp { get; set; }

        [Required]
        public int ChangeCounter { get; set; }
    }
}
