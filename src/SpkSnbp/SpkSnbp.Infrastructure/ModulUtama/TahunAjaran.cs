using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class TahunAjaranConfiguration : IEntityTypeConfiguration<TahunAjaran>
{
    public void Configure(EntityTypeBuilder<TahunAjaran> builder)
    {
        builder.HasMany(x => x.DaftarSiswa).WithOne(y => y.TahunAjaran);
    }
}

internal class TahunAjaranRepository : ITahunAjaranRepository
{
    private readonly AppDbContext _appDbContext;

    public TahunAjaranRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(TahunAjaran tahunAjaran) => _appDbContext.TahunAjaran.Add(tahunAjaran);

    public void Delete(TahunAjaran tahunAjaran) => _appDbContext.TahunAjaran.Remove(tahunAjaran);

    public async Task<TahunAjaran?> Get(int id) => await _appDbContext
        .TahunAjaran
        .Include(x => x.DaftarSiswa)
        .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<TahunAjaran>> GetAll() => await _appDbContext
        .TahunAjaran
        .Include(x => x.DaftarSiswa)
        .ToListAsync();

    public void Update(TahunAjaran tahunAjaran) => _appDbContext.TahunAjaran.Update(tahunAjaran);
}
