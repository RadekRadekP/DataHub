using System;
using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models.UI
{
    // Model určený specificky pro Blazor UI komponenty
    public class AlarmUIModel
    {
        // Server-generated primary key (pro editaci/mazání existujících)
        public int Id { get; set; }

        // ID z klientské databáze (pro zobrazení nebo identifikaci na straně klienta)
        public int ClientDbId { get; set; }

        // Identifikátor klienta
        [Required(ErrorMessage = "Client ID je povinné.")]
        public string ClientId { get; set; } = string.Empty;

        // Datum a čas alarmu
        [Required(ErrorMessage = "Datum alarmu je povinné.")]
        public DateTime AlarmDate { get; set; }

        // Typ alarmu
        [Required(ErrorMessage = "Typ alarmu je povinný.")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        // Číslo alarmu
        [Required(ErrorMessage = "Číslo alarmu je povinné.")]
        public int Nr { get; set; }

        // Text alarmu
        [Required(ErrorMessage = "Text alarmu je povinný.")]
        [MaxLength(255)]
        public string Text { get; set; } = string.Empty;

        // Operátor
        [Required(ErrorMessage = "Operátor je povinný.")]
        [MaxLength(50)]
        public string Operator { get; set; } = string.Empty;

        // Časové razítko serveru
        [EditableAttribute(false)]
        public DateTime ServerTimestamp { get; set; }

        // Počítadlo změn
        [EditableAttribute(false)]
        public int ChangeCounter { get; set; }
        

        // Zde můžete přidat další vlastnosti specifické pro UI, např.
        // public bool IsAcknowledged { get; set; }
        // public string DisplayDate => AlarmDate.ToString("yyyy-MM-dd HH:mm");
    }
}