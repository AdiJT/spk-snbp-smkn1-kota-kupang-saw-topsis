using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.MataPelajaran;

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

    [Display(Name = "Mata Pelajaran Kejuruan")]
    [Range(0d, 100d, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1} dan {2}")]
    public double? MataPelajaranKejuruan { get; set; }

    [Display(Name = "Mata Pelajaran Umum")]
    [Range(0d, 100d, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1} dan {2}")]
    public double? MataPelajaranUmum { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            MataPelajaranKejuruan = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan)?.Nilai,
            MataPelajaranUmum = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum)?.Nilai
        }).ToList();
}
