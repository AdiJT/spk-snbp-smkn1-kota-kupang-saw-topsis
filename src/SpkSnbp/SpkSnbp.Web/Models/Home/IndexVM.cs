using SpkSnbp.Domain.ModulUtama;

namespace SpkSnbp.Web.Models.Home;

public class IndexVM
{
    public required List<Siswa> DaftarSiswa { get; set; } = [];
    public required List<TahunAjaran> DaftarTahunAjaran { get; set; } = [];
    public required List<Kriteria> DaftarKriteria { get; set; } = [];
}
