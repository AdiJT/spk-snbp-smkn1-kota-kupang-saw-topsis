using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class Kelas : Entity<int>
{
    public required string Nama { get; set; }

    public List<Siswa> DaftarSiswa { get; set; } = [];
}

public interface IKelasRepository
{
    Task<Kelas?> Get(int id);
    Task<Kelas?> GetFirst();
    Task<List<Kelas>> GetAll();
    Task<bool> IsExist(string nama, int? id = null);

    void Add(Kelas kelas);
    void Update(Kelas kelas);
    void Delete(Kelas kelas);
}