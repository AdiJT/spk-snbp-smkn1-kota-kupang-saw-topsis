using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class SiswaKriteriaConfiguration : IEntityTypeConfiguration<SiswaKriteria>
{
    public void Configure(EntityTypeBuilder<SiswaKriteria> builder)
    {
        builder.HasKey(x => new { x.IdSiswa, x.IdKriteria });

        builder.HasOne(x => x.Siswa).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdSiswa);
        builder.HasOne(x => x.Kriteria).WithMany(y => y.DaftarSiswaKriteria).HasForeignKey(x => x.IdKriteria);
    }
}

internal class SiswaKriteriaRepository : ISiswaKriteriaRepository
{
    private readonly AppDbContext _appDbContext;

    public SiswaKriteriaRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Add(SiswaKriteria siswaKriteria) => _appDbContext.SiswaKriteria.Add(siswaKriteria);

    public void Delete(SiswaKriteria siswaKriteria) => _appDbContext.SiswaKriteria.Remove(siswaKriteria);

    public void Update(SiswaKriteria siswaKriteria) => _appDbContext.SiswaKriteria.Update(siswaKriteria);
}
