using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class SiswaKriteria : JoinEntity
{
    public int IdSiswa { get; set; }
    public int IdKriteria { get; set; }
    public required double Nilai { get; set; }

    public Siswa Siswa { get; set; }
    public Kriteria Kriteria { get; set; }

    protected override IEnumerable<object> GetJoinKeys() => [IdSiswa, IdKriteria];
}

public interface ISiswaKriteriaRepository
{
    void Add(SiswaKriteria siswaKriteria);
    void Update(SiswaKriteria siswaKriteria);
    void Delete(SiswaKriteria siswaKriteria);
}