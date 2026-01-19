using SpkSnbp.Domain.Abstracts;

namespace SpkSnbp.Domain.ModulUtama;

public class InformasiSekolah : Entity<int>
{
    public static readonly InformasiSekolah Default = new()
    {
        NPSN = "50304998",
        NamaSekolah = "SMKN1 Kota Kupang",
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
}

public interface IInformasiSekolahRepository
{
    Task<InformasiSekolah> Get();
    void Update(InformasiSekolah informasiSekolah);
}
