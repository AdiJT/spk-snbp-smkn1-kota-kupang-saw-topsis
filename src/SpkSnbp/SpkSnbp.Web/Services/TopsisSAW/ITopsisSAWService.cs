using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Web.Services.TopsisSAW;

public interface ITopsisSAWService
{
    Task<Result<HasilPerhitungan>> Perhitungan(int tahun, Jurusan jurusan);
    Task<Result> SeleksiEligible(int tahun, Jurusan jurusan);
}