using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.InformasiSekolahModels;

public class EditVM
{
    [Display(Name = "NPSN")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string NPSN { get; set; }

    [Display(Name = "Nama Sekolah")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string NamaSekolah { get; set; }

    [Display(Name = "Bentuk Pendidikan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string BentukPendidikan { get; set; }

    [Display(Name = "Akreditasi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Akreditasi { get; set; }

    [Display(Name = "Nilai")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required double Nilai { get; set; }

    [Display(Name = "No. SK Akreditasi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string NoSKAkreditasi { get; set; }

    [Display(Name = "Tanggal SK Akreditasi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required DateOnly TanggalSKAkreditasi { get; set; }

    [Display(Name = "TMT Mulai SK Akreditasi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required DateOnly TMTMulaiSKAkreditasi { get; set; }

    [Display(Name = "TMT Selesai SK Akreditasi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required DateOnly TMTSelesaiSKAkreditasi { get; set; }

    [Display(Name = "Kepala Sekolah")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string KepalaSekolah { get; set; }

    [Display(Name = "No. HP")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string NoHP { get; set; }

    [Display(Name = "Jalan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Jalan { get; set; }

    [Display(Name = "Desa/Kelurahan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string DesaKelurahan { get; set; }

    [Display(Name = "Kecamatan/Distrik")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string KecamatanDistrik { get; set; }

    [Display(Name = "Kabupaten/Kota")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string KabupatenKota { get; set; }

    [Display(Name = "Provinsi")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Provinsi { get; set; }

    [Display(Name = "Kode Pos")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string KodePos { get; set; }
}
