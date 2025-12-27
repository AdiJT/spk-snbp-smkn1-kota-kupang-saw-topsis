using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Models.Absensi;

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
    public required double? Absensi { get; set; }

    [Display(Name = "Jumlah Absen")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(0, 45, MaximumIsExclusive = false, ErrorMessage = "{0} harus antara {1}-{2}")]
    public int JumlahAbsen { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            Absensi = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.Absensi)?.Nilai
        }).ToList();
}
