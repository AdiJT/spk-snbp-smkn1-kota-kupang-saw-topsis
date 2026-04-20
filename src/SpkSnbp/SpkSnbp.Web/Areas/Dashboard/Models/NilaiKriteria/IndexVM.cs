using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.NilaiKriteria;

public class IndexVM
{
    public int? Id { get; set; }
    public Kriteria? Kriteria { get; set; }

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

    [Display(Name = "Nilai")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required double? Nilai { get; set; }
}

public static class EnumerableExtensions 
{
    public static List<IndexEntryVM> ToIndexEntryList(this IEnumerable<Siswa> daftarSiswa, int? idKriteria) => [.. daftarSiswa
        .Select(x => new IndexEntryVM
        {
            Siswa = x,
            IdSiswa = x.Id,
            Nilai = idKriteria is null ? null : x.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == idKriteria)?.Nilai,
        })];
}
