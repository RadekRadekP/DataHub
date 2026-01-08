using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RPK_BlazorApp.Models.Interfaces; // Přidáno pro IAuditableEntity

namespace RPK_BlazorApp.Models
{
    [Table("AllarmData")] // Změněno na "AllarmData", aby odpovídalo skutečnému názvu tabulky v DB
    public class Alarm  : IAuditableEntity, ICloneable // Přejmenováno z Allarm na Alarm pro C# konvence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Internal, auto-generated ID

        [Column(Order = 1)]
        [MaxLength(50)]
        public string ClientId { get; set; }= string.Empty;

        [Column(Order = 2)]
        public int ClientDbId { get; set; } // ID from the client (renamed)

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }= string.Empty;

        [Required]
        [MaxLength(255)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Operator { get; set; } = string.Empty;

        public DateTime ServerTimestamp { get; set; }
        
        [Column("allarmDate")] // Explicitně mapuje tuto vlastnost na databázový sloupec "allarmDate"
        public DateTime AlarmDate { get; set; }
        [Required]
        public int Nr { get; set; } = 0;
        [Required]
        public int ChangeCounter { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
