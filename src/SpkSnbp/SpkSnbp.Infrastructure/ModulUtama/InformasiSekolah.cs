using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Infrastructure.Database;

namespace SpkSnbp.Infrastructure.ModulUtama;

internal class InformasiSekolahConfiguration : IEntityTypeConfiguration<InformasiSekolah>
{
    public void Configure(EntityTypeBuilder<InformasiSekolah> builder)
    {
        builder.HasKey(x => x.Id);
    }
}

//internal class InformasiSekolahRepository : IInformasiSekolahRepository
//{
//    private readonly AppDbContext _appDbContext;

//    public InformasiSekolahRepository(AppDbContext appDbContext)
//    {
//        _appDbContext = appDbContext;
//    }

//    public async Task<InformasiSekolah> Get()
//    {
//        var informasiSekolah = await _appDbContext.InformasiSekolah.FirstOrDefaultAsync();
//        if (informasiSekolah is null)
//        {
//            informasiSekolah = InformasiSekolah.Default;
//            _appDbContext.InformasiSekolah.Add(informasiSekolah);
//            await _appDbContext.SaveChangesAsync();
//        }
//    }

//    public void Update(InformasiSekolah informasiSekolah)
//    {
//        throw new NotImplementedException();
//    }
//}
