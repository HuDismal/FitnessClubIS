using System.Windows;
using FitnessClubIS.Models;
using FitnessClubIS.ViewModels;

namespace FitnessClubIS.Views;

public partial class ClientEditWindow : Window
{
    public ClientEditWindow(Client client)
    {
        InitializeComponent();
        DataContext = new ClientEditViewModel(client);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (((ClientEditViewModel)DataContext).Save())
            DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
