using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class KelasConfiguration : IEntityTypeConfiguration<Kelas>
{
    public void Configure(EntityTypeBuilder<Kelas> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.DaftarSiswa).WithOne(y => y.Kelas);

        builder.HasData(
            new Kelas
            {
                Id = 1,
                Nama = "1"
            },
            new Kelas
            {
                Id = 2,
                Nama = "2"
            },
            new Kelas
            {
                Id = 3,
                Nama = "3"
            },
            new Kelas
            {
                Id = 4,
                Nama = "4"
            },
            new Kelas
            {
                Id = 5,
                Nama = "5"
            }
        );
    }
}

internal class KelasRepository : IKelasRepository
{
    private readonly AppDbContext _appDbContext;

    public KelasRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(Kelas kelas) => _appDbContext.Kelas.Add(kelas);

    public void Delete(Kelas kelas) => _appDbContext.Kelas.Remove(kelas);

    public async Task<Kelas?> Get(int id) => await _appDbContext
        .Kelas
        .Include(x => x.DaftarSiswa)
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Kelas>> GetAll() => await _appDbContext
        .Kelas
        .Include(x => x.DaftarSiswa)
        .ToListAsync();

    public async Task<Kelas?> GetFirst() => await _appDbContext
        .Kelas
        .Include(x => x.DaftarSiswa)
        .OrderBy(x => x.Nama)
        .FirstOrDefaultAsync();

    public async Task<bool> IsExist(string nama, int? id = null) => await _appDbContext
        .Kelas
        .AnyAsync(x => x.Id != id && x.Nama.ToLower() == nama.ToLower());

    public void Update(Kelas kelas) => _appDbContext.Kelas.Update(kelas);
}