using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Models.Ekstrakulikuler;

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
    public required double? Ekstrakulikuler { get; set; }

    [Display(Name = "Ekstrakulikuler")]
    public PredikatEkstrakulikuler? Ekstrakulikuler1 { get; set; }

    [Display(Name = "Ekstrakulikuler")]
    public PredikatEkstrakulikuler? Ekstrakulikuler2 { get; set; }

    [Display(Name = "Ekstrakulikuler")]
    public PredikatEkstrakulikuler? Ekstrakulikuler3 { get; set; }

    public List<PredikatEkstrakulikuler> DaftarEkskul => 
        [.. new List<PredikatEkstrakulikuler?> { Ekstrakulikuler1, Ekstrakulikuler2, Ekstrakulikuler3}.Where(x => x.HasValue).Select(x => x!.Value)];
}

public enum PredikatEkstrakulikuler
{
    SangatKurang = 1,
    Kurang = 2,
    Cukup = 3,
    Baik = 4,
    SangatBaik = 5
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa) => daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            Ekstrakulikuler = x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.Ekstrakulikuler)?.Nilai
        }).ToList();
}
