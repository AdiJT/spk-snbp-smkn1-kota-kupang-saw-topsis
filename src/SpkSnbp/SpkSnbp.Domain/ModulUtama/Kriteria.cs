using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class Kriteria : Entity<int>
{
    public required string Nama { get; set; }
    public required int Bobot { get; set; }
    public required JenisKriteria Jenis { get; set; }

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