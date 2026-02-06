using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.SertifikatLSPModels;

public class IndexVM
{
    public int? Tahun { get; set; }
    public TahunAjaran? TahunAjaran { get; set; }

    public int? IdKelas { get; set; }
    public Kelas? Kelas { get; set; }

    public Jurusan? Jurusan { get; set; }

    public List<IndexEntryVM> DaftarEntry { get; set; } = [];
}

public class IndexEntryVM
{
    public required Siswa Siswa { get; set; }
    public required int IdSiswa { get; set; }

    [Display(Name = "Sertifikat LSP")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required SertifikatLSP? SertifikatLSP { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            SertifikatLSP = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.SertLSP)?.Nilai switch
            {
                1 => SertifikatLSP.BelumKompeten,
                5 => SertifikatLSP.Kompeten,
                _ => null
            }
        }).ToList();
}
