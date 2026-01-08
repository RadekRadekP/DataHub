using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    public class ClientIdentifier
    {
        [Key] // Definuje ClientId jako primární klíč
        [Required(ErrorMessage = "Client ID je povinné.")]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Název je povinný.")]
        [Display(Name = "Client Název")]
        public string ClientName { get; set; } = string.Empty;

        // Případně další vlastnosti, pokud by byly potřeba
    }
}
