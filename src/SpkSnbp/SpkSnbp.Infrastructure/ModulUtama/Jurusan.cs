using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class JurusanConfiguration : IEntityTypeConfiguration<Jurusan>
{
    public void Configure(EntityTypeBuilder<Jurusan> builder)
    {
        builder.HasMany(x => x.DaftarSiswa).WithOne(y => y.Jurusan); 
    }
}

internal class JurusanRepository : IJurusanRepository
{
    private readonly AppDbContext _appDbContext;

    public JurusanRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(Jurusan jurusan) => _appDbContext.Jurusan.Add(jurusan);

    public void Delete(Jurusan jurusan) => _appDbContext.Jurusan.Remove(jurusan);

    public async Task<Jurusan?> Get(int id) => await _appDbContext
        .Jurusan
        .Include(x => x.DaftarSiswa)
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Jurusan>> GetAll() => await _appDbContext
        .Jurusan
        .Include(x => x.DaftarSiswa)
        .ToListAsync();

    public void Update(Jurusan jurusan) => _appDbContext.Jurusan.Update(jurusan);
}
