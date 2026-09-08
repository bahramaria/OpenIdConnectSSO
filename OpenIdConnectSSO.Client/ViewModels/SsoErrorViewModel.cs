namespace OpenIdConnectSSO.Client.ViewModels;

public class SsoErrorViewModel
{
    public string Type { get; set; } = string.Empty;
    public string ErrorId { get; set; } = string.Empty;

    public string Title => Type switch
    {
        "remote" => "خطا در ارتباط با سرویس احراز هویت",
        "auth" => "خطا در فرآیند احراز هویت",
        "system" => "خطای سیستمی",
        _ => "خطای ورود"
    };

    public string Message => Type switch
    {
        "remote" => "ارتباط با سرویس ورود یکپارچه با مشکل مواجه شد.",
        "auth" => "اعتبارسنجی توکن انجام نشد.",
        "system" => "خطای غیرمنتظره در سیستم رخ داده است.",
        _ => "فرآیند ورود انجام نشد."
    };
}