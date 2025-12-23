using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class Siswa : Entity<int>
{
    public required string NISN { get; set; }
    public required string Nama { get; set; }
    public double? NilaiTopsis { get; set; }
    public Eligible? Eligible { get; set; }
    public required Jurusan Jurusan { get; set; }

    public TahunAjaran TahunAjaran { get; set; }

    public List<Kriteria> DaftarKriteria { get; set; } = [];
    public List<SiswaKriteria> DaftarSiswaKriteria { get; set; } = [];
}

public interface ISiswaRepository
{
    Task<Siswa?> Get(int id);
    Task<List<Siswa>> GetAll(Jurusan? jurusan = null, int? tahunAjaran = null);
    Task<bool> IsExist(string nisn, int? idFilter = null);

    void Add(Siswa siswa);
    void Update(Siswa siswa);
    void Delete(Siswa siswa);
}

public enum Eligible
{
    Ya, Tidak
}

public enum Jurusan
{
    TJKT, Akuntasi, Perkantoran, Pariwisata, Pemasaran
}