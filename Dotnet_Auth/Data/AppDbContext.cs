using System;
using System.Collections.Generic;
using Dotnet_Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_Auth.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<common_data> common_data { get; set; }

    public virtual DbSet<user_info> user_infos { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<common_data>(entity =>
        {
            entity.HasKey(e => e.id).HasName("common_data_pkey");

            entity.ToTable("common_data", "security");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.comment).HasColumnType("character varying");
            entity.Property(e => e.is_active).HasDefaultValue(true);

            entity.HasOne(d => d.user).WithMany(p => p.common_data)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("common_data_user_id_fkey");
        });

        modelBuilder.Entity<user_info>(entity =>
        {
            entity.HasKey(e => e.id).HasName("user_info_pkey");

            entity.ToTable("user_info", "security");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.email).HasMaxLength(200);
            entity.Property(e => e.password).HasMaxLength(600);
            entity.Property(e => e.user_name).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
