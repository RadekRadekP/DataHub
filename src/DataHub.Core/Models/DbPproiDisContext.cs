using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataHub.Core.Models;

public partial class DbPproiDisContext : DbContext
{
    public DbPproiDisContext()
    {
    }

    public DbPproiDisContext(DbContextOptions<DbPproiDisContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ViewEisodSd> ViewEisodSds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=NTSPPR;Database=dbPPRoiDIS;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ViewEisodSd>(entity =>
        {
            entity.ToView("VIEW_EISOD_SD", "export");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
