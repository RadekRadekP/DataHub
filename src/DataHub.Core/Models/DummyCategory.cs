using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    /// <summary>
    /// Lookup table for DummyItem categories (in-memory)
    /// </summary>
    public class DummyCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ColorCode { get; set; }
    }
}
