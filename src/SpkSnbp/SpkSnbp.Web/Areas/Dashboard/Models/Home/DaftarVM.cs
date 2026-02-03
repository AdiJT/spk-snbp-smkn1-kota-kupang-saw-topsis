using System.ComponentModel.DataAnnotations;

namespace SpkSnbp.Web.Areas.Dashboard.Models.Home;

public class DaftarVM
{
    [Display(Name = "User Name")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "{0} harus diisi")]
    [MinLength(8, ErrorMessage = "Panjang minimal 8")]
    [RegularExpression(
        pattern: "^.*(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\\W]).*$",
        ErrorMessage = "Password harus berisi huruf besar, huruf kecil, angka, dan karakter khusus")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Konfirmasi Password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "{0} harus diisi")]
    [Compare(nameof(Password), ErrorMessage = "{0} harus sama dengan {1}")]
    public string KonfirmasiPassword { get; set; } = string.Empty;

    [Display(Name = "Role")]
    [Required(ErrorMessage = "{0} harus diisi")]
    public string Role { get; set; } = string.Empty;
}
