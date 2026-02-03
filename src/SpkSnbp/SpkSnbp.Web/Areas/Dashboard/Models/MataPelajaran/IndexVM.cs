using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.MataPelajaran;

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

    [Display(Name = "Mata Pelajaran Kejuruan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(0d, 100d, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1} dan {2}")]
    public required double MataPelajaranKejuruan { get; set; }

    [Display(Name = "Mata Pelajaran Umum")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(0d, 100d, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1} dan {2}")]
    public required double MataPelajaranUmum { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            MataPelajaranKejuruan = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan)?.Nilai ?? 0d,
            MataPelajaranUmum = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum)?.Nilai ?? 0d
        }).ToList();
}
