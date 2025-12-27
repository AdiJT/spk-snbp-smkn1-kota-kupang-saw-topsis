using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class HasilPerhitungan : Entity<int>
{
    public required Jurusan Jurusan { get; set; }
    public required DateTime TanggalPerhitungan { get; set; }
    public required double[,] MatriksPerhitungan { get; set; }
    public required double[,] MatriksTernormalisasi { get; set; }
    public required double[] MaxKriteria { get; set; }
    public required double[] MinKriteria { get; set; }
    public required double[,] MatriksTernormalisasiTerbobot { get; set; }
    public required double[] SolusiIdealPositif { get; set; }
    public required double[] SolusiIdealNegatif { get; set; }
    public required double[] JarakSolusiIdealPositif { get; set; }
    public required double[] JarakSolusiIdealNegatif { get; set; }

    public TahunAjaran TahunAjaran { get; set; }
}

public interface IHasilPerhitunganRepository
{
    Task<HasilPerhitungan?> Get(int tahun, Jurusan jurusan);
    void Add(HasilPerhitungan hasilPerhitungan);
    void Delete(HasilPerhitungan hasilPerhitungan);
}
