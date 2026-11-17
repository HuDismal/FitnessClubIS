using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FitnessClubIS.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected string _title;
    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }
}

/// <summary>Позволяет ViewModel получить пароль из PasswordBox, не зная о самом элементе управления.</summary>
public interface IGettingPassword
{
    string GetPassword();
}
