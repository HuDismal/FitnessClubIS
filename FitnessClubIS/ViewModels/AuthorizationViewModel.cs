using System.Security.Cryptography;
using System.Text;
using FitnessClubIS.Data;
using FitnessClubIS.Models;
using FitnessClubIS.Services;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubIS.ViewModels;

public class AuthorizationViewModel : ViewModelBase
{
    /// <summary>Текущий авторизованный пользователь (доступен всему приложению).</summary>
    public static Account Account { get; private set; }

    private const string CaptchaAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public AuthorizationViewModel()
    {
        Title = "Фитнес-клуб «Атлет»";
        RefreshCaptcha();
    }

    public IGettingPassword GettingPassword { private get; set; }
    private string Password => GettingPassword?.GetPassword() ?? string.Empty;

    private string _login = string.Empty;
    public string Login { get => _login; set => Set(ref _login, value); }

    private string _captchaText;
    public string CaptchaText { get => _captchaText; private set => Set(ref _captchaText, value); }

    private string _captchaInput = string.Empty;
    public string CaptchaInput { get => _captchaInput; set => Set(ref _captchaInput, value); }

    /// <summary>Сообщение об ошибке для отображения на форме.</summary>
    private string _error;
    public string Error { get => _error; private set => Set(ref _error, value); }

    public void RefreshCaptcha()
    {
        var chars = new char[8];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = CaptchaAlphabet[RandomNumberGenerator.GetInt32(CaptchaAlphabet.Length)];
        CaptchaText = new string(chars);
        CaptchaInput = string.Empty;
    }

    public static string Hash(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();

    public bool LogIn()
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrEmpty(Password))
        {
            Error = "Введите логин и пароль";
            return false;
        }
        // капча сравнивается с учётом регистра
        if (CaptchaInput != CaptchaText)
        {
            Error = "Неверно введена капча";
            RefreshCaptcha();
            return false;
        }

        try
        {
            using var db = new FitnessContext();
            var hash = Hash(Password);
            var account = db.Accounts.Include(a => a.AccountType)
                .FirstOrDefault(a => a.Login == Login && a.PasswordHash == hash);
            if (account == null)
            {
                Error = "Неверный логин или пароль";
                RefreshCaptcha();          // при каждой неудачной попытке — новая капча
                return false;
            }
            Account = account;
            return true;
        }
        catch (Exception ex)
        {
            AppServices.Instance.Get<IDialogService>()
                .ShowDialog("Ошибка", "Не удалось подключиться к базе данных:\n" + ex.Message, DialogType.Error);
            return false;
        }
    }

    public static void LogOut() => Account = null;
}
