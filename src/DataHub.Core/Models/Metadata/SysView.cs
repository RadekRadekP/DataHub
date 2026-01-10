using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models.Metadata
{
    /// <summary>
    /// Represents a specific View (Presentation) of a MetaEntity.
    /// E.g., "Default Grid", "Admin Form", "Mobile Card".
    /// </summary>
    public class SysView
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The entity this view displays.
        /// Works for both SQL and Memory entities.
        /// </summary>
        [Required]
        public int MetaEntityId { get; set; }

        [ForeignKey(nameof(MetaEntityId))]
        public MetaEntity? MetaEntity { get; set; }

        /// <summary>
        /// Name of the view (e.g., "UserGrid", "QuickEdit")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "Default";

        /// <summary>
        /// View Type: "Grid", "Form", "Card", etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ViewType { get; set; } = "Grid";

        /// <summary>
        /// Description of what this view is for.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Is this the default view for this Entity + ViewType?
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// JSON configuration for Layout (e.g. Columns per row, Grouping)
        /// </summary>
        public string? LayoutConfig { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The fields configured in this view.
        /// </summary>
        public ICollection<SysViewField> Fields { get; set; } = new List<SysViewField>();
    }
}
