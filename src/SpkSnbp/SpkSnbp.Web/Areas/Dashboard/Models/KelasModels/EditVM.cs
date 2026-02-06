using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.KelasModels;

public class EditVM
{
    public required int Id { get; set; }

    [Display(Name = "Nama")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public required string Nama { get; set; }
}
