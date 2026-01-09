using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataHub.Core.Models.Interfaces;

namespace Grinding.Shared.Models
{
    public class Grinding : IAuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Internal, auto-generated ID

        [Column(Order = 1)]
        [MaxLength(50)]
        public string ClientId { get; set;} = string.Empty;

        [Column(Order = 2)]
        public int ClientDbId { get; set; }= 0; // ID from the client (renamed)

        [Required]
        [MaxLength(50)]
        public string ProgramName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Operator { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Lotto { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string GwType { get; set; } = string.Empty; // Initialize with default

        public DateTime ServerTimestamp { get; set; }

        [Required]
        public TimeSpan GrindingTime { get; set; }
        [Required]
        public TimeSpan FinishTime { get; set; }
        public DateTime DateStart { get; set; }
        public double UpperGWStart { get; set; }
        public double LowerGWStart { get; set; }

        // --- Add this line ---
        [Required]
        public int ChangeCounter { get; set; } // Initialize to 1 for the first creation
        // ---------------------
    }
}
