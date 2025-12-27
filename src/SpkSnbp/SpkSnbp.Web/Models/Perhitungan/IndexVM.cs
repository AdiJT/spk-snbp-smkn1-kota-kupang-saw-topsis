using SpkSnbp.Domain.ModulUtama;

namespace SpkSnbp.Web.Models.Perhitungan;

public class IndexVM
{
    public required Jurusan Jurusan { get; set; }

    public int? Tahun { get; set; }
    public TahunAjaran? TahunAjaran { get; set; }

    public required List<Siswa> DaftarSiswa { get; set; }
    public HasilPerhitungan? HasilPerhitungan { get; set; }
}
