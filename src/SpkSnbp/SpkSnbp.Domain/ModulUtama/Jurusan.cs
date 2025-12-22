using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class Jurusan : Entity<int>
{
    public required string Nama { get; set; }

    public List<Siswa> DaftarSiswa { get; set; } = [];
}

public interface IJurusanRepository
{
    Task<Jurusan?> Get(int id);
    Task<List<Jurusan>> GetAll();

    void Add(Jurusan jurusan);
    void Update(Jurusan jurusan);
    void Delete(Jurusan jurusan);
}