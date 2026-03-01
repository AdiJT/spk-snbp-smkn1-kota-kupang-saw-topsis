using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class InformasiSekolah : Entity<int>
{
    public static readonly InformasiSekolah Default = new()
    {
        Id = 1,
        NPSN = "50304998",
        NamaSekolah = "SMKN 1 Kota Kupang",
        BentukPendidikan = "SMK",
        Akreditasi = "A",
        Nilai = 92,
        NoSKAkreditasi = "1857/BAN-SM/SK/2022",
        TanggalSKAkreditasi = new(2022, 11, 30),
        TMTMulaiSKAkreditasi = new(2022, 11, 30),
        TMTSelesaiSKAkreditasi = new(2027, 12, 31),
        KepalaSekolah = "Mixon Rudolf Nicolas Abineno",
        NoHP = "081313474144",
        Jalan = "JL. PROF. DR. W. Z. JOHANES",
        DesaKelurahan = "Oetete",
        KecamatanDistrik = "Kec. Oebobo",
        KabupatenKota = "Kota Kupang",
        Provinsi = "Prov. Nusa Tenggara Timur",
        KodePos = "85112",
        Visi = "<p>\r\n                ”Menjadi Lembaga Pendidikan dan Pelatihan Bisnis Manajemen, Pariwisata, dan Teknologi Informatika dan Komunikasi Berstandar Nasional dan Internasional serta Mampu Menghasilkan Tamatan yang Berdaya Saing di \tEra Global.”\r\n            </p>",
        Misi = "<ol>\r\n                <li>Menumbuhkan semangat keunggulan dan komparatif secara intensif kepada seluruh warga sekolah. </li>\r\n                <li>Melaksanakan kegiatan belajar mengajar secara optimal yang berorientasi kepada pencapaian kompetensi berstandar nasional/internasional dengan tetap mempertimbangkan potensi yang dimiliki peserta didik. </li>\r\n                <li>Mengembangkan dan mengintensifkan hubungan sekolah dengan DUDI dan institusi lain yang telah memiliki reputasi nasional/internasional sebagai perwujudan dari <em>demand driven</em>.</li>\r\n                <li>Menerapkan pengelolaan manajemen SMK mengacu pada standar ISO 9001 2008 dengan melibatkan seluruh warga sekolah dan <em>pelanggan</em>. </li>\r\n            </ol>"
    };

    public required string NPSN { get; set; }
    public required string NamaSekolah { get; set; }
    public required string BentukPendidikan { get; set; }
    public required string Akreditasi { get; set; }
    public required double Nilai { get; set; }
    public required string NoSKAkreditasi { get; set; } 
    public required DateOnly TanggalSKAkreditasi { get; set; } 
    public required DateOnly TMTMulaiSKAkreditasi { get; set; } 
    public required DateOnly TMTSelesaiSKAkreditasi { get; set; } 
    public required string KepalaSekolah { get; set; } 
    public required string NoHP { get; set; } 
    public required string Jalan { get; set; } 
    public required string DesaKelurahan { get; set; } 
    public required string KecamatanDistrik { get; set; } 
    public required string KabupatenKota { get; set; } 
    public required string Provinsi { get; set; } 
    public required string KodePos { get; set; } 
    public required string Visi { get; set; } 
    public required string Misi { get; set; } 
}

public interface IInformasiSekolahRepository
{
    Task<InformasiSekolah> Get();
    void Update(InformasiSekolah informasiSekolah);
}
