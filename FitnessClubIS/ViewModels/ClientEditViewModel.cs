using System.Text.RegularExpressions;
using FitnessClubIS.Data;
using FitnessClubIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubIS.ViewModels;

public class ClientEditViewModel : ViewModelBase
{
    private readonly int? _id;

    public ClientEditViewModel(Client client)
    {
        _id = client?.Id;
        Title = client == null ? "Новый клиент" : "Редактирование клиента";
        LastName = client?.LastName;
        FirstName = client?.FirstName;
        Patronymic = client?.Patronymic;
        BirthDate = client?.BirthDate ?? DateTime.Today.AddYears(-25);
        Phone = client?.Phone;
        Email = client?.Email;
    }

    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Patronymic { get; set; }
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    private string _error;
    public string Error { get => _error; private set => Set(ref _error, value); }

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(LastName)) return "Укажите фамилию";
        if (string.IsNullOrWhiteSpace(FirstName)) return "Укажите имя";
        if (BirthDate > DateTime.Today.AddYears(-14)) return "Клиенту должно быть не менее 14 лет";
        if (string.IsNullOrWhiteSpace(Phone) || !Regex.IsMatch(Phone, @"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$"))
            return "Телефон должен быть в формате +7 (999) 123-45-67";
        if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return "Некорректный адрес электронной почты";
        return null;
    }

    /// <summary>Сохраняет клиента (добавление или изменение). Возвращает true при успехе.</summary>
    public bool Save()
    {
        Error = Validate();
        if (Error != null) return false;

        try
        {
            using var db = new FitnessContext();
            Client entity;
            if (_id == null)
            {
                entity = new Client { RegisteredAt = DateTime.Today };
                db.Clients.Add(entity);
            }
            else
            {
                entity = db.Clients.Find(_id.Value);
            }
            entity.LastName = LastName.Trim();
            entity.FirstName = FirstName.Trim();
            entity.Patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic.Trim();
            entity.BirthDate = BirthDate.Date;
            entity.Phone = Phone.Trim();
            entity.Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
            db.SaveChanges();
            return true;
        }
        catch (DbUpdateException)
        {
            Error = "Клиент с таким телефоном или e-mail уже существует";
            return false;
        }
        catch (Exception ex)
        {
            Error = "Ошибка сохранения: " + ex.Message;
            return false;
        }
    }
}
