using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class TahunAjaran : Entity<int>
{
    public List<Siswa> DaftarSiswa { get; set; } = [];
}

public interface ITahunAjaranRepository
{
    Task<TahunAjaran?> Get(int id);
    Task<List<TahunAjaran>> GetAll();

    void Add(TahunAjaran tahunAjaran);
    void Update(TahunAjaran tahunAjaran);
    void Delete(TahunAjaran tahunAjaran);
}