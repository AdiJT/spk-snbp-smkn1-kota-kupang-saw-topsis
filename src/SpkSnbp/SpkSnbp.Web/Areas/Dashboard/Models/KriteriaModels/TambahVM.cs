using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.KriteriaModels;

public class TambahVM
{
    [Display(Name = "Nama")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public string Nama { get; set; } = string.Empty;

    [Display(Name = "Bobot")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(1, int.MaxValue, ErrorMessage = "{0} harus lebih besar dari {1}")]
    public int Bobot { get; set; }

    [Display(Name = "Jenis")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public JenisKriteria Jenis { get; set; }

    public string? ReturnUrl { get; set; }
}
