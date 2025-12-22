using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class SiswaConfiguration : IEntityTypeConfiguration<Siswa>
{
    public void Configure(EntityTypeBuilder<Siswa> builder)
    {
        builder.HasOne(x => x.TahunAjaran).WithMany(y => y.DaftarSiswa);
        builder.HasOne(x => x.Jurusan).WithMany(y => y.DaftarSiswa);

        builder
            .HasMany(x => x.DaftarKriteria)
            .WithMany(y => y.DaftarSiswa)
            .UsingEntity<SiswaKriteria>(
                l => l.HasOne(x => x.Kriteria).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdKriteria),
                r => r.HasOne(x => x.Siswa).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdSiswa)
            );
    }
}

internal class SiswaRepository : ISiswaRepository
{
    private readonly AppDbContext _appDbContext;

    public SiswaRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(Siswa siswa) => _appDbContext.Siswa.Add(siswa);

    public void Delete(Siswa siswa) => _appDbContext.Siswa.Remove(siswa);

    public async Task<Siswa?> Get(int id) => await _appDbContext
        .Siswa
        .Include(x => x.TahunAjaran)
        .Include(x => x.Jurusan)
        .Include(x => x.DaftarKriteria)
        .Include(x => x.DaftarSiswaKriteria).ThenInclude(y => y.Kriteria)
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Siswa>> GetAll() => await _appDbContext
        .Siswa
        .Include(x => x.TahunAjaran)
        .Include(x => x.Jurusan)
        .Include(x => x.DaftarKriteria)
        .Include(x => x.DaftarSiswaKriteria).ThenInclude(y => y.Kriteria)
        .ToListAsync();

    public async Task<List<Siswa>> GetAll(int tahunAjaran, int idJurusan) => await _appDbContext
        .Siswa
        .Include(x => x.TahunAjaran)
        .Include(x => x.Jurusan)
        .Include(x => x.DaftarKriteria)
        .Include(x => x.DaftarSiswaKriteria).ThenInclude(y => y.Kriteria)
        .Where(x => x.TahunAjaran.Id == tahunAjaran && x.Jurusan.Id == idJurusan)
        .ToListAsync();

    public async Task<bool> IsExist(string nisn, int? idFilter = null) => await _appDbContext
        .Siswa
        .AnyAsync(x => x.Id != idFilter && x.NISN == nisn);

    public void Update(Siswa siswa) => _appDbContext.Siswa.Update(siswa);
}
