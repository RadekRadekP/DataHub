using System;
using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models.UI
{
    public class ViewEisodSdUIModel
    {
        // A unique string ID created by combining the composite key fields.
        // This simplifies handling in the UI components.
        public string Id => $"{InterníKódVzorce}-{ČísloOperace}-{ČísloPoložky}";

        [Display(Name = "Číslo operace")]
        public int? ČísloOperace { get; set; }

        [Display(Name = "Číslo položky")]
        public string ČísloPoložky { get; set; } = null!;

        [Display(Name = "Číslo vzorce")]
        public int ČísloVzorce { get; set; }

        [Display(Name = "Druh operace")]
        public string? DruhOperace { get; set; }

        [Display(Name = "Hodnota")]
        public string? Hodnota { get; set; }

        [Display(Name = "Interní kód vzorce")]
        public int InterníKódVzorce { get; set; }

        [Display(Name = "Key OVV")]
        public int? KeyOvv { get; set; }

        [Display(Name = "Skupina operace")]
        public string? SkupinaOperace { get; set; }

        [Display(Name = "Typ operace")]
        public string? TypOperace { get; set; }

        [Display(Name = "Vzorec")]
        public string Vzorec { get; set; } = null!;

        public string? TruncatedHodnota => Hodnota != null && Hodnota.Length > 50 ? Hodnota.Substring(0, 50) + "..." : Hodnota;
        public string? TruncatedVzorec => Vzorec != null && Vzorec.Length > 50 ? Vzorec.Substring(0, 50) + "..." : Vzorec;

        [Display(Name = "Zobrazit v SD")]
        public int ZobrazitVSd { get; set; }
    }
}