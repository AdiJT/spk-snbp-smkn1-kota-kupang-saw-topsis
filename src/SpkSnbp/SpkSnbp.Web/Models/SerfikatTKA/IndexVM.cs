using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Models.SertifikatTKA;

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

    [Display(Name = "Sertifikat TKA")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(1, 5, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1}-{2}")]
    public required int SertifikatTKA { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            SertifikatTKA = (int?)x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.SertTKA)?.Nilai ?? 0
        }).ToList();
}
