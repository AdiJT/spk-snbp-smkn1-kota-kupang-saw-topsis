using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Web.Services.TopsisSAW;

public class TopsisSAWService : ITopsisSAWService
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHasilPerhitunganRepository _hasilPerhitunganRepository;

    public TopsisSAWService(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IHasilPerhitunganRepository hasilPerhitunganRepository)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _hasilPerhitunganRepository = hasilPerhitunganRepository;
    }

    public async Task<Result<HasilPerhitungan>> Perhitungan(int tahun, Jurusan jurusan)
    {
        var tahunAjaran = await _tahunAjaranRepository.Get(tahun);
        if (tahunAjaran is null) return new Error("Perhitungan.TahunTidakditemukan", "Tahun tidak ditemukan");

        var daftarSiswa = await _siswaRepository.GetAll(jurusan, tahun);
        if (daftarSiswa.Count == 0)
            return new Error("Perhitungan.SiswaTidakAda", "Tidak ada data siswa");
        daftarSiswa = daftarSiswa.OrderBy(x => x.NISN).ToList();

        var daftarKriteria = await _kriteriaRepository.GetAll();
        if (daftarKriteria.Count == 0)
            return new Error("Perhitungan.KriteriaTidakAda", "Tidak ada data kriteria");
        daftarKriteria = daftarKriteria.OrderBy(x => x.Id).ToList();

        var matriks = new double[daftarSiswa.Count, daftarKriteria.Count];
        var maxKriteria = new double[daftarKriteria.Count];
        var minKriteria = new double[daftarKriteria.Count];
        var matriksTernormalisasi = new double[daftarSiswa.Count, daftarKriteria.Count];
        var matriksTernormalisasiTerbobot = new double[daftarSiswa.Count, daftarKriteria.Count];
        var solusiIdealPositif = new double[daftarKriteria.Count];
        var solusiIdealNegatif = new double[daftarKriteria.Count];
        var jarakSolusiIdealPositif = new double[daftarSiswa.Count];
        var jarakSolusiIdealNegatif = new double[daftarSiswa.Count];

        try
        {
            for (int j = 0; j < daftarKriteria.Count; j++)
            {
                maxKriteria[j] = double.MinValue;
                minKriteria[j] = double.MaxValue;
            }

            for (int i = 0; i < daftarSiswa.Count; i++)
            {
                var siswa = daftarSiswa[i];
                for (int j = 0; j < daftarKriteria.Count; j++)
                {
                    var kriteria = daftarKriteria[j];
                    var siswaKriteria = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.Kriteria == kriteria);
                    if (siswaKriteria is null)
                        return new Error("Perhitungan.DataTidakLengkap", $"Siswa {siswa.Nama} ({siswa.NISN}) tidak memiliki data kriteria {kriteria.Nama}");

                    matriks[i, j] = siswaKriteria.Nilai;

                    if (matriks[i, j] > maxKriteria[j])
                        maxKriteria[j] = matriks[i, j];

                    if (matriks[i, j] < minKriteria[j])
                        minKriteria[j] = matriks[i, j];
                }
            }

            for (int i = 0; i < daftarSiswa.Count; i++)
            {
                var siswa = daftarSiswa[i];
                for (int j = 0; j < daftarKriteria.Count; j++)
                {
                    var kriteria = daftarKriteria[j];
                    if (kriteria.Jenis == JenisKriteria.Benefit)
                        matriksTernormalisasi[i, j] = matriks[i, j] / maxKriteria[j];
                    else
                        matriksTernormalisasi[i, j] = minKriteria[j] / matriks[i, j];
                }
            }

            for (int j = 0; j < daftarKriteria.Count; j++)
            {
                var kriteria = daftarKriteria[j];

                if (kriteria.Jenis == JenisKriteria.Benefit)
                {
                    solusiIdealPositif[j] = int.MinValue;
                    solusiIdealNegatif[j] = int.MaxValue;
                }
                else
                {
                    solusiIdealPositif[j] = int.MaxValue;
                    solusiIdealNegatif[j] = int.MinValue;
                }
            }

            for (int i = 0; i < daftarSiswa.Count; i++)
            {
                var siswa = daftarSiswa[i];
                for (int j = 0; j < daftarKriteria.Count; j++)
                {
                    var kriteria = daftarKriteria[j];
                    matriksTernormalisasiTerbobot[i, j] = matriksTernormalisasi[i, j] * kriteria.Bobot;

                    if (kriteria.Jenis == JenisKriteria.Benefit)
                    {
                        if (matriksTernormalisasiTerbobot[i, j] > solusiIdealPositif[j])
                            solusiIdealPositif[j] = matriksTernormalisasiTerbobot[i, j];

                        if (matriksTernormalisasiTerbobot[i, j] < solusiIdealNegatif[j])
                            solusiIdealNegatif[j] = matriksTernormalisasiTerbobot[i, j];
                    }
                    else
                    {
                        if (matriksTernormalisasiTerbobot[i, j] < solusiIdealPositif[j])
                            solusiIdealPositif[j] = matriksTernormalisasiTerbobot[i, j];

                        if (matriksTernormalisasiTerbobot[i, j] > solusiIdealNegatif[j])
                            solusiIdealNegatif[j] = matriksTernormalisasiTerbobot[i, j];
                    }
                }
            }

            for (int i = 0; i < daftarSiswa.Count; i++)
            {
                var siswa = daftarSiswa[i];
                var totalSelisihKuadratPositif = 0d;
                var totalSelisihKuadratNegatif = 0d;

                for (int j = 0; j < daftarKriteria.Count; j++)
                {
                    var kriteria = daftarKriteria[j];
                    totalSelisihKuadratPositif += Math.Pow(solusiIdealPositif[j] - matriksTernormalisasiTerbobot[i, j], 2);
                    totalSelisihKuadratNegatif += Math.Pow(matriksTernormalisasiTerbobot[i, j] - solusiIdealNegatif[j], 2);
                }

                jarakSolusiIdealPositif[i] = Math.Sqrt(totalSelisihKuadratPositif);
                jarakSolusiIdealNegatif[i] = Math.Sqrt(totalSelisihKuadratNegatif);
            }

            for (int i = 0; i < daftarSiswa.Count; i++)
            {
                var siswa = daftarSiswa[i];
                siswa.NilaiTopsis = jarakSolusiIdealNegatif[i] / (jarakSolusiIdealNegatif[i] + jarakSolusiIdealPositif[i]);

                _siswaRepository.Update(siswa);
            }
        }
        catch (Exception ex)
        {
            return new Error("Perhitungan.TerjadiError", ex.Message);
        }

        var hasilPerhitungan = await _hasilPerhitunganRepository.Get(tahun, jurusan);
        if (hasilPerhitungan is not null)
            _hasilPerhitunganRepository.Delete(hasilPerhitungan);

        hasilPerhitungan = new HasilPerhitungan
        {
            TahunAjaran = tahunAjaran,
            Jurusan = jurusan,
            TanggalPerhitungan = CultureInfos.DateTimeNow,
            MatriksPerhitungan = matriks,
            MaxKriteria = maxKriteria,
            MinKriteria = minKriteria,
            MatriksTernormalisasi = matriksTernormalisasi,
            MatriksTernormalisasiTerbobot = matriksTernormalisasiTerbobot,
            SolusiIdealPositif = solusiIdealPositif,
            SolusiIdealNegatif = solusiIdealNegatif,
            JarakSolusiIdealPositif = jarakSolusiIdealPositif,
            JarakSolusiIdealNegatif = jarakSolusiIdealNegatif,
            DaftarSiswa = daftarSiswa
        };

        _hasilPerhitunganRepository.Add(hasilPerhitungan);

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsFailure) return result.Error;

        return hasilPerhitungan;
    }

    public Task<Result> SeleksiEligible(int tahun, Jurusan jurusan)
    {
        throw new NotImplementedException();
    }
}
