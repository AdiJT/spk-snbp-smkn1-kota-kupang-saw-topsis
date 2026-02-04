using SpkSnbp.Domain.Abstracts;
using System.ComponentModel;

namespace SpkSnbp.Domain.ModulUtama;

public class Kriteria : Entity<int>
{
    public required string Nama { get; set; }
    public required int Bobot { get; set; }
    public required JenisKriteria Jenis { get; set; }

    public string Kode => $"C{Id}";

    public List<Siswa> DaftarSiswa { get; set; } = [];
    public List<SiswaKriteria> DaftarSiswaKriteria { get; set; } = [];
}

public interface IKriteriaRepository
{
    Task<Kriteria?> Get(int id);
    Task<List<Kriteria>> GetAll();

    void Update(Kriteria kriteria);
}

public enum JenisKriteria
{
    Benefit, Cost
}

public enum KriteriaEnum
{
    [Description("Mata Pelajaran Kejuruan")]
    MPKejuruan = 1,

    [Description("Mata Pelajaran Umum")]
    MPUmum = 2,

    [Description("Sertifikat LSP")]
    SertLSP = 3,

    [Description("Sertifikat TKA")]
    SertTKA = 4,

    [Description("Ekstrakurikuler")]
    Ekstrakulikuler = 5,

    [Description("Absensi")]
    Absensi = 6
}

public enum SertifikatLSP
{
    BelumKompeten = 1, Kompeten = 5
}

public enum PredikatEkstrakulikuler
{
    SangatKurang = 1,
    Kurang = 2,
    Cukup = 3,
    Baik = 4,
    SangatBaik = 5
}
