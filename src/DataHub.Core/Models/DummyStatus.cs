using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    /// <summary>
    /// Lookup table for DummyItem status (in-memory)
    /// </summary>
    public class DummyStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Is this a terminal status (workflow complete)?
        /// </summary>
        public bool IsTerminal { get; set; }
    }
}
