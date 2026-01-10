using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models.Metadata
{
    /// <summary>
    /// Represents a column/property in a MetaEntity.
    /// </summary>
    public class MetaField
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to the parent entity
        /// </summary>
        [Required]
        public int MetaEntityId { get; set; }

        [ForeignKey(nameof(MetaEntityId))]
        public MetaEntity? MetaEntity { get; set; }

        /// <summary>
        /// The property name in C# (e.g., "AlarmDate", "LocationName")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The column name in SQL Server
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Display name for UI (e.g., "Alarm Date", "Location")
        /// </summary>
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The SQL Server data type (e.g., "nvarchar", "int", "datetime2")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SqlType { get; set; } = string.Empty;

        /// <summary>
        /// The CLR type (e.g., "System.String", "System.Int32", "System.DateTime")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ClrType { get; set; } = string.Empty;

        /// <summary>
        /// Is this field part of the primary key?
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Is this field a foreign key to another entity?
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Is this field nullable in the database?
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Is this field computed or generated?
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Maximum length for string fields
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Precision for numeric fields
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Scale for numeric fields
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Default value expression
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Sort order for display (0 = first)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Should this field be visible in the grid by default?
        /// </summary>
        public bool IsVisibleInGrid { get; set; } = true;

        /// <summary>
        /// Should this field be editable in forms?
        /// </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// UI hint for rendering (e.g., "TextBox", "DatePicker", "Lookup", "ColorPicker")
        /// </summary>
        [MaxLength(100)]
        public string? UiHint { get; set; }

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
