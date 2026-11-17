using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FitnessClubIS.Data;
using FitnessClubIS.Models;
using FitnessClubIS.Services;
using FitnessClubIS.Views;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubIS.ViewModels;

public class ClientsViewModel : ViewModelBase
{
    private readonly IDialogService _dialog = AppServices.Instance.Get<IDialogService>();

    public ObservableCollection<Client> Clients { get; } = new();

    public List<string> Filters { get; } = new()
    {
        "Все клиенты", "С действующим абонементом", "Без действующего абонемента"
    };
    public List<string> Sorts { get; } = new()
    {
        "По фамилии (А–Я)", "По фамилии (Я–А)",
        "По дате регистрации (новые сверху)", "По дате регистрации (старые сверху)"
    };

    // ---------- поиск ----------
    private string _searchString = string.Empty;
    public string SearchString
    {
        get => _searchString;
        set { if (Set(ref _searchString, value)) UpdateClientList(); }
    }

    // ---------- фильтрация ----------
    private int _selectedFilter;
    public int SelectedFilter
    {
        get => _selectedFilter;
        set { if (Set(ref _selectedFilter, value)) UpdateClientList(); }
    }

    // ---------- сортировка ----------
    private int _selectedSort;
    public int SelectedSort
    {
        get => _selectedSort;
        set { if (Set(ref _selectedSort, value)) UpdateClientList(); }
    }

    private Client _selectedClient;
    public Client SelectedClient
    {
        get => _selectedClient;
        set { if (Set(ref _selectedClient, value)) { EditCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged(); } }
    }

    private string _counter;
    public string Counter { get => _counter; private set => Set(ref _counter, value); }

    // ---------- команды ----------
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ResetCommand { get; }

    public ClientsViewModel()
    {
        Title = "Клиенты";
        AddCommand = new RelayCommand(() => OpenEditor(null));
        EditCommand = new RelayCommand(() => OpenEditor(SelectedClient), () => SelectedClient != null);
        DeleteCommand = new RelayCommand(Delete, () => SelectedClient != null);
        ResetCommand = new RelayCommand(Reset);
        UpdateClientList();
    }

    private void Reset()
    {
        _searchString = string.Empty; _selectedFilter = 0; _selectedSort = 0;
        OnPropertyChanged(nameof(SearchString));
        OnPropertyChanged(nameof(SelectedFilter));
        OnPropertyChanged(nameof(SelectedSort));
        UpdateClientList();
    }

    public void UpdateClientList()
    {
        try
        {
            using var db = new FitnessContext();
            var today = DateTime.Today;
            IQueryable<Client> query = db.Clients
                .Include(c => c.Memberships).ThenInclude(m => m.MembershipType)
                .AsNoTracking();

            // поиск по фамилии, имени или телефону
            var s = SearchString?.Trim();
            if (!string.IsNullOrEmpty(s))
            {
                var pattern = $"%{s}%";
                query = query.Where(c => EF.Functions.ILike(c.LastName, pattern)
                                      || EF.Functions.ILike(c.FirstName, pattern)
                                      || EF.Functions.ILike(c.Phone, pattern));
            }

            // фильтр по наличию действующего абонемента
            query = SelectedFilter switch
            {
                1 => query.Where(c => c.Memberships.Any(m => m.StartDate <= today && m.EndDate >= today)),
                2 => query.Where(c => !c.Memberships.Any(m => m.StartDate <= today && m.EndDate >= today)),
                _ => query
            };

            // сортировка
            query = SelectedSort switch
            {
                1 => query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName),
                2 => query.OrderByDescending(c => c.RegisteredAt),
                3 => query.OrderBy(c => c.RegisteredAt),
                _ => query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            };

            var list = query.ToList();
            Clients.Clear();
            foreach (var c in list) Clients.Add(c);
            Counter = $"Найдено клиентов: {list.Count}";
        }
        catch (Exception ex)
        {
            _dialog.ShowDialog("Ошибка", "Не удалось загрузить список клиентов:\n" + ex.Message, DialogType.Error);
        }
    }

    private void OpenEditor(Client client)
    {
        var window = new ClientEditWindow(client) { Owner = System.Windows.Application.Current.MainWindow };
        if (window.ShowDialog() == true) UpdateClientList();
    }

    private void Delete()
    {
        if (SelectedClient == null) return;
        var answer = _dialog.ShowDialog("Удаление клиента",
            $"Удалить клиента «{SelectedClient.FullName}»?\nВместе с ним будут удалены его абонементы, платежи и записи на занятия.",
            DialogType.Question);
        if (answer != DialogResultType.Yes) return;

        try
        {
            using var db = new FitnessContext();
            var entity = db.Clients.Find(SelectedClient.Id);
            if (entity != null)
            {
                db.Clients.Remove(entity);
                db.SaveChanges();    // зависимые строки удаляет СУБД (ON DELETE CASCADE)
            }
            UpdateClientList();
        }
        catch (Exception ex)
        {
            _dialog.ShowDialog("Ошибка", "Не удалось удалить клиента:\n" + ex.Message, DialogType.Error);
        }
    }
}
