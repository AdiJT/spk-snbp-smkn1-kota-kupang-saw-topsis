using SpkSnbp.Domain.Abstracts;
using System.ComponentModel;

namespace SpkSnbp.Domain.ModulUtama;

public class Kriteria : Entity<int>
{
    public required string Nama { get; set; }
    public required Bobot Bobot { get; set; }
    public required JenisKriteria Jenis { get; set; }
    public required bool IsDefault { get; set; }
    public required bool Active { get; set; }

    public string Kode => $"C{Id}";

    public List<Siswa> DaftarSiswa { get; set; } = [];
    public List<SiswaKriteria> DaftarSiswaKriteria { get; set; } = [];
}

public interface IKriteriaRepository
{
    Task<Kriteria?> Get(int id);
    Task<List<Kriteria>> GetAll();
    Task<List<Kriteria>> GetAllActive();
    Task<bool> IsExist(string nama, int? id = default);

    void Add(Kriteria kriteria);
    void Delete(Kriteria kriteria);
    void Update(Kriteria kriteria);
}

public enum Bobot
{
    [Description("Kriteria memiliki pengaruh sangat kecil terhadap hasil seleksi")]
    SangatRendah = 1,

    [Description("Kriteria berpengaruh kecil dalam proses penilaian")]
    Rendah = 2,

    [Description("Kriteria memiliki pengaruh yang cukup terhadap hasil keputusan")]
    Cukup = 3,

    [Description("Kriteria dianggap penting dan berpengaruh besar")]
    Tinggi = 4,

    [Description("Kriteria memiliki pengaruh dominan dalam menentukan hasil akhir")]
    SangatTinggi = 5,
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
