using SpkSnbp.Domain.ModulUtama;
using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models;

public class ImportVM
{
    public required int Tahun { get; set; }
    public required Jurusan Jurusan { get; set; }

    [Display(Name = "File (.xlxs)")]
    [Required(ErrorMessage = "{0} harus diupload")]
    public IFormFile? FormFile { get; set; }

    public string? ReturnUrl { get; set; }
}
