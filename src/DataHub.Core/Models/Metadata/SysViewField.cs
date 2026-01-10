using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models.Metadata
{
    /// <summary>
    /// Configuration for a specific field within a SysView.
    /// Overrides default display properties for the context of this View.
    /// </summary>
    public class SysViewField
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The View this field belongs to.
        /// </summary>
        [Required]
        public int SysViewId { get; set; }

        [ForeignKey(nameof(SysViewId))]
        public SysView? SysView { get; set; }

        /// <summary>
        /// The Data Field (Source of Truth).
        /// </summary>
        [Required]
        public int MetaFieldId { get; set; }

        [ForeignKey(nameof(MetaFieldId))]
        public MetaField? MetaField { get; set; }

        /// <summary>
        /// UI Label. Defaults to MetaField.DisplayName if null.
        /// </summary>
        [MaxLength(200)]
        public string? CustomLabel { get; set; }

        /// <summary>
        /// Order in the View (0 = First).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Is this field visible in this view?
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Read-only in this view?
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// UI Hint for this view (e.g. "ColorPicker").
        /// Overrides MetaField.UiHint.
        /// </summary>
        [MaxLength(100)]
        public string? CustomUiHint { get; set; }

        /// <summary>
        /// Width configuration (e.g. "200px", "1fr"). Useful for Grids.
        /// </summary>
        [MaxLength(50)]
        public string? Width { get; set; }

        /// <summary>
        /// Grouping for Forms (e.g. "General Info", "Address").
        /// </summary>
        [MaxLength(100)]
        public string? GroupName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
