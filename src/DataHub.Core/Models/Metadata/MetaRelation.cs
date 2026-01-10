using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models.Metadata
{
    /// <summary>
    /// Represents a relationship (foreign key) between two entities.
    /// </summary>
    public class MetaRelation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The entity that contains the foreign key (the "child" or "dependent" entity)
        /// </summary>
        [Required]
        public int FromEntityId { get; set; }

        [ForeignKey(nameof(FromEntityId))]
        public MetaEntity? FromEntity { get; set; }

        /// <summary>
        /// The field in the FromEntity that is the foreign key
        /// </summary>
        [Required]
        public int FromFieldId { get; set; }

        [ForeignKey(nameof(FromFieldId))]
        public MetaField? FromField { get; set; }

        /// <summary>
        /// The entity being referenced (the "parent" or "principal" entity)
        /// </summary>
        [Required]
        public int ToEntityId { get; set; }

        [ForeignKey(nameof(ToEntityId))]
        public MetaEntity? ToEntity { get; set; }

        /// <summary>
        /// The field in the ToEntity that is the primary key (usually Id)
        /// </summary>
        [Required]
        public int ToFieldId { get; set; }

        [ForeignKey(nameof(ToFieldId))]
        public MetaField? ToField { get; set; }

        /// <summary>
        /// Which field in the ToEntity should be displayed in lookups (e.g., "Name", "Title")
        /// </summary>
        public int? DisplayFieldId { get; set; }

        [ForeignKey(nameof(DisplayFieldId))]
        public MetaField? DisplayField { get; set; }

        /// <summary>
        /// Relationship type: "OneToMany", "ManyToOne", "ManyToMany"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RelationType { get; set; } = "ManyToOne";

        /// <summary>
        /// Display name for this relationship (e.g., "Customer", "Order Items")
        /// </summary>
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Delete behavior: "Cascade", "SetNull", "Restrict"
        /// </summary>
        [MaxLength(50)]
        public string? DeleteBehavior { get; set; }

        /// <summary>
        /// When this metadata entry was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this metadata entry was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
