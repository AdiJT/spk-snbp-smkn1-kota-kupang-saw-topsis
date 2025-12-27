using SpkSnbp.Domain.Abstracts;

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
    MPKejuruan = 1,
    MPUmum = 2,
    SertLSP = 3,
    SertTKA = 4,
    Ekstrakulikuler = 5,
    Absensi = 6
}