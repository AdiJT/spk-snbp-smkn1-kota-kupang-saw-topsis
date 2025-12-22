using Microsoft.EntityFrameworkCore;
using SpkSnbp.Domain.Abstracts;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;

namespace SpkSnbp.Infrastructure.Database;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.CreatedAt))
                    .HasColumnType("timestamp without time zone");

                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.UpdatedAt))
                    .HasColumnType("timestamp without time zone");
            }

            if (typeof(Entity<>).IsAssignableFrom(entityType.ClrType))
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasKey("Id");
        }

        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> User { get; set; }
    public DbSet<TahunAjaran> TahunAjaran { get; set; }
    public DbSet<Jurusan> Jurusan { get; set; }
    public DbSet<Siswa> Siswa { get; set; }
    public DbSet<Kriteria> Kriteria { get; set; }
    public DbSet<SiswaKriteria> SiswaKriteria { get; set; }
}
