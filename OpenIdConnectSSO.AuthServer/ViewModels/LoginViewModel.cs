using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenIdConnectSSO.AuthServer.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "*")]
    [MaxLength(50, ErrorMessage = "*")]
    [UIHint("Username")]
    //[DisplayName("نام کاربری")]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "*")]
    [MaxLength(50, ErrorMessage = "*")]
    [UIHint("Password")]
    //[DisplayName("رمز ورود")]
    public string Password { get; set; } = string.Empty;


    [DisplayName("Remember me")]
    public bool IsPersistent { get; set; }


    //public string? ReturnUrl { get; set; } = string.Empty;
}