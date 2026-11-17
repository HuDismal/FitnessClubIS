using FitnessClubIS.Models;
using FitnessClubIS.Views.Pages;

namespace FitnessClubIS.ViewModels;

public class NavItem
{
    public string Title { get; init; }
    public string Icon { get; init; }
    public Type PageType { get; init; }
}

public class MainWindowViewModel : ViewModelBase
{
    public Account Account { get; }
    public List<NavItem> NavItems { get; }

    private NavItem _selectedItem;
    public NavItem SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

    public MainWindowViewModel()
    {
        Title = "Фитнес-клуб «Атлет»";
        Account = AuthorizationViewModel.Account;

        // разделы меню зависят от типа учётной записи
        var all = new Dictionary<string, NavItem>
        {
            ["clients"]  = new NavItem { Title = "Клиенты",     Icon = "👥", PageType = typeof(ClientsPage) },
            ["schedule"] = new NavItem { Title = "Расписание",  Icon = "📅", PageType = typeof(SchedulePage) },
            ["reports"]  = new NavItem { Title = "Отчёты",      Icon = "📊", PageType = typeof(ReportsPage) },
        };
        NavItems = Account?.AccountType?.Name switch
        {
            "Администратор" => new() { all["clients"], all["schedule"], all["reports"] },
            "Менеджер"      => new() { all["clients"], all["reports"] },
            "Тренер"        => new() { all["schedule"] },
            _               => new()
        };
        SelectedItem = NavItems.FirstOrDefault();
    }
}
