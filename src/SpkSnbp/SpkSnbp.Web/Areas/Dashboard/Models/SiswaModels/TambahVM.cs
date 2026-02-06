using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.SiswaModels;

public class TambahVM
{
    [Display(Name = "NISN")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public string NISN { get; set; } = string.Empty;

    [Display(Name = "Nama")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public string Nama { get; set; } = string.Empty;

    [Display(Name = "Jurusan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public Jurusan Jurusan { get; set; }

    [Display(Name = "Kelas")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public int IdKelas { get; set; }

    [Display(Name = "Tahun Ajaran")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public int IdTahunAjaran { get; set; }

    public string? ReturnUrl { get; set; }
}
