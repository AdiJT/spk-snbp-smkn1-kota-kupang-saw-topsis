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
                Id = (int)KriteriaEnum.MPKejuruan,
                Nama = "Mata Pelajaran Kejuruan",
                Bobot = 5,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.MPUmum,
                Nama = "Mata Pelajaran Umum",
                Bobot = 4,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.SertLSP,
                Nama = "Sertifikat LSP",
                Bobot = 4,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.SertTKA,
                Nama = "Sertifikat TKA",
                Bobot = 3,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.Ekstrakulikuler,
                Nama = "Ekstrakurikuler",
                Bobot = 2,
                Jenis = JenisKriteria.Benefit
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.Presensi,
                Nama = "Presensi",
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
        .OrderBy(x => x.Id)
        .ToListAsync();

    public void Update(Kriteria kriteria) => _appDbContext.Kriteria.Update(kriteria);
}