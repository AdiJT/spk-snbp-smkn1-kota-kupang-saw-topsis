using SpkSnbp.Domain.ModulUtama;

namespace SpkSnbp.Web.Areas.Dashboard.Models.SiswaModels;

public class IndexVM
{
    public int? Tahun { get; set; }
    public TahunAjaran? TahunAjaran { get; set; }

    public Jurusan? Jurusan { get; set; }

    public int? IdKelas { get; set; }
    public Kelas? Kelas { get; set; }

    public List<Siswa> DaftarSiswa { get; set; } = [];
}
