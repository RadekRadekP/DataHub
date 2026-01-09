using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Eisod.Shared.Models;

// The [Keyless] attribute is removed. The key is now defined in EisodDbContext.cs using the Fluent API.
public partial class ViewEisodSd
{
    [Column("Číslo vzorce")]
    public int ČísloVzorce { get; set; }

    [StringLength(255)]
    public string Vzorec { get; set; } = null!;

    [Column("Číslo položky")]
    [StringLength(50)]
    public string ČísloPoložky { get; set; } = null!;

    [Column("Skupina operace")]
    [StringLength(255)]
    public string? SkupinaOperace { get; set; }

    [Column("Druh operace")]
    [StringLength(255)]
    public string? DruhOperace { get; set; }

    [Column("Typ operace")]
    [StringLength(255)]
    public string? TypOperace { get; set; }

    [Column("Key OVV")]
    public int? KeyOvv { get; set; }

    // A property that is part of a composite key cannot be nullable.
    [Column("Číslo operace")]
    public int? ČísloOperace { get; set; }

    [StringLength(4000)]
    public string? Hodnota { get; set; }

    [Column("Zobrazit v SD")]
    public int ZobrazitVSd { get; set; }

    [Column("Interní kód vzorce")]
    public int InterníKódVzorce { get; set; }
}
