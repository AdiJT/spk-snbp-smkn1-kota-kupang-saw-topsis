using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class HasilPerhitunganConfiguration : IEntityTypeConfiguration<HasilPerhitungan>
{
    public void Configure(EntityTypeBuilder<HasilPerhitungan> builder)
    {
        builder.HasOne(x => x.TahunAjaran).WithMany(y => y.DaftarHasil);
        builder.Property(x => x.TanggalPerhitungan).HasColumnType("timestamp without time zone");
    }
}

internal class HasilPerhitunganRepository : IHasilPerhitunganRepository
{
    private readonly AppDbContext _appDbContext;

    public HasilPerhitunganRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(HasilPerhitungan hasilPerhitungan) => _appDbContext.HasilPerhitungan.Add(hasilPerhitungan);

    public void Delete(HasilPerhitungan hasilPerhitungan) => _appDbContext.HasilPerhitungan.Remove(hasilPerhitungan);

    public async Task<HasilPerhitungan?> Get(int tahun, Jurusan jurusan) => await _appDbContext
        .HasilPerhitungan
        .Include(x => x.TahunAjaran)
        .FirstOrDefaultAsync(x => x.TahunAjaran.Id == tahun && x.Jurusan == jurusan);
}
