using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.KriteriaModels;

public class EditVM
{
    public required int Id { get; set; }

    [Display(Name = "Bobot")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(1, int.MaxValue, ErrorMessage = "{0} harus lebih besar dari {1}")]
    public required int Bobot { get; set; }

    [Display(Name = "Jenis")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required JenisKriteria Jenis { get; set; }

    public string? ReturnUrl { get; set; }
}
