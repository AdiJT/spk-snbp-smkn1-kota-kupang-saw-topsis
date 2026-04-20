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
                Bobot = Bobot.SangatTinggi,
                Jenis = JenisKriteria.Benefit,
                IsDefault = true,
                Active = true
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.MPUmum,
                Nama = "Mata Pelajaran Umum",
                Bobot = Bobot.Tinggi,
                Jenis = JenisKriteria.Benefit,
                IsDefault = true,
                Active = true
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.SertLSP,
                Nama = "Sertifikat LSP",
                Bobot = Bobot.Tinggi,
                Jenis = JenisKriteria.Benefit,
                IsDefault = true,
                Active = true
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.SertTKA,
                Nama = "Sertifikat TKA",
                Bobot = Bobot.Cukup,
                Jenis = JenisKriteria.Benefit,
                IsDefault = true,
                Active = true
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.Ekstrakulikuler,
                Nama = "Ekstrakurikuler",
                Bobot = Bobot.Rendah,
                Jenis = JenisKriteria.Benefit,
                IsDefault = true,
                Active = true
            },
            new Kriteria
            {
                Id = (int)KriteriaEnum.Absensi,
                Nama = "Absensi",
                Bobot = Bobot.SangatRendah,
                Jenis = JenisKriteria.Cost,
                IsDefault = true,
                Active = true
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

    public void Add(Kriteria kriteria) => _appDbContext.Kriteria.Add(kriteria);

    public void Delete(Kriteria kriteria) => _appDbContext.Kriteria.Remove(kriteria);

    public async Task<Kriteria?> Get(int id) => await _appDbContext
        .Kriteria
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Kriteria>> GetAll() => await _appDbContext
        .Kriteria
        .OrderBy(x => x.Id)
        .ToListAsync();

    public async Task<List<Kriteria>> GetAllActive() => await _appDbContext
        .Kriteria
        .Where(x => x.Active)
        .OrderBy(x => x.Id)
        .ToListAsync();

    public async Task<bool> IsExist(string nama, int? id = default) => await _appDbContext
        .Kriteria
        .AnyAsync(x => x.Id != id && x.Nama.ToLower() == nama.ToLower());

    public void Update(Kriteria kriteria) => _appDbContext.Kriteria.Update(kriteria);
}