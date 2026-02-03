using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.SertifikatTKA;

public class IndexVM
{
    public int? Tahun { get; set; }
    public TahunAjaran? TahunAjaran { get; set; }

    public Jurusan? Jurusan { get; set; }

    public List<IndexEntryVM> DaftarEntry { get; set; } = [];
}

public class IndexEntryVM
{
    public required Siswa Siswa { get; set; }
    public required int IdSiswa { get; set; }
    public required double? SertifikatTKA { get; set; }

    [Display(Name = "Skor")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(0d, 100d, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1}-{2}")]
    public double Skor { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            SertifikatTKA = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.SertTKA)?.Nilai,
            Skor = x.SkorTKA ?? 0,
        }).ToList();
}
