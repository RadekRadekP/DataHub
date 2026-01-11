using DataHub.Core.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models
{
    /// <summary>
    /// DummyMeta entity - a clean implementation for metadata-driven UI development
    /// </summary>
    public class DummyMeta : ICloneable, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        [MaxLength(20)]
        public string? CustomTag { get; set; }

        /// <summary>
        /// Foreign key to DummyCategory
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Navigation property (for metadata discovery)
        /// </summary>
        [ForeignKey(nameof(CategoryId))]
        public DummyCategory? Category { get; set; }

        /// <summary>
        /// Foreign key to DummyStatus
        /// </summary>
        public int? StatusId { get; set; }

        /// <summary>
        /// Navigation property (for metadata discovery)
        /// </summary>
        [ForeignKey(nameof(StatusId))]
        public DummyStatus? Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; }

        public decimal? Value { get; set; }

        // IAuditableEntity properties
        public DateTime ServerTimestamp { get; set; }
        public int ChangeCounter { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
