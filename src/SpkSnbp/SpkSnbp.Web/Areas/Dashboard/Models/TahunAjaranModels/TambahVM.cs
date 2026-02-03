using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.TahunAjaranModels;

public class TambahVM
{
    [Display(Name = "Tahun")]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Range(1, int.MaxValue, ErrorMessage = "{0} tidak boleh negatif")]
    public int Tahun { get; set; }

    public string? ReturnUrl { get; set; }
}
