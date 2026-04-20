using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.KriteriaModels;

public class EditVM
{
    public required int Id { get; set; }

    [Display(Name = "Nama")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Nama { get; set; }

    [Display(Name = "Bobot")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required Bobot Bobot { get; set; }

    [Display(Name = "Jenis")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required JenisKriteria Jenis { get; set; }

    public string? ReturnUrl { get; set; }
}
