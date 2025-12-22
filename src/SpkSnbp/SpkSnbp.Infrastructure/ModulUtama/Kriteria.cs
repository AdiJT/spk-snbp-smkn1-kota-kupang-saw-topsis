using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class KriteriaConfiguration : IEntityTypeConfiguration<Kriteria>
{
    public void Configure(EntityTypeBuilder<Kriteria> builder)
    {
        builder
            .HasMany(x => x.DaftarSiswa)
            .WithMany(y => y.DaftarKriteria)
            .UsingEntity<SiswaKriteria>(
                l => l.HasOne(x => x.Siswa).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdSiswa),
                r => r.HasOne(x => x.Kriteria).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdKriteria)
            );

        builder.HasData(
            new Kriteria
            {
                Id = 1,
                Nama = "Mata Pelajaran Kejuruan",
                Bobot = 5,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = 2,
                Nama = "Mata Pelajaran Umum",
                Bobot = 4,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = 3,
                Nama = "Sertifikat LSP",
                Bobot = 4,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = 4,
                Nama = "Sertifikat TKA",
                Bobot = 3,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = 5,
                Nama = "Ekstrakurikuler",
                Bobot = 2,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = 6,
                Nama = "Absensi",
                Bobot = 1,
                Jenis = JenisKriteria.Cost
            }
        );
    }
}

internal class KriteriaRepository : IKriteriaRepository
{
    private readonly AppDbContext _appDbContext;

    public KriteriaRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Kriteria?> Get(int id) => await _appDbContext
        .Kriteria
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Kriteria>> GetAll() => await _appDbContext
        .Kriteria
        .ToListAsync();

    public void Update(Kriteria kriteria) => _appDbContext.Kriteria.Update(kriteria);
}