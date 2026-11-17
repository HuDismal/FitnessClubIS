using System.Windows;
using System.Windows.Controls;
using FitnessClubIS.ViewModels;

namespace FitnessClubIS.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Navigate();
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e) => Navigate();

    private void Navigate()
    {
        if (MainFrame == null) return;   // событие может прийти до создания Frame
        if (DataContext is MainWindowViewModel { SelectedItem: { } item })
            MainFrame.Navigate(Activator.CreateInstance(item.PageType));
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();
}
