using RPK_BlazorApp.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace RPK_BlazorApp.Models
{
    public class DummyItem : ICloneable, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; }

        // IAuditableEntity properties
        public DateTime ServerTimestamp { get; set; }
        public int ChangeCounter { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}