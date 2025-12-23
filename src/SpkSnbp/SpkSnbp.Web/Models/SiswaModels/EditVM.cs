using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Models.SiswaModels;

public class EditVM
{
    public required int Id { get; set; }

    [Display(Name = "NISN")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string NISN { get; set; }

    [Display(Name = "Nama")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Nama { get; set; }

    [Display(Name = "Jurusan")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required Jurusan Jurusan { get; set; }

    [Display(Name = "Tahun Ajaran")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required int IdTahunAjaran { get; set; }

    public string? ReturnUrl { get; set; }
}
