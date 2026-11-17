using System.Windows;
using FitnessClubIS.Views;

namespace FitnessClubIS.Services;

public class DialogService : IDialogService
{
    public DialogResultType ShowDialog(string title, string message, DialogType type)
    {
        var dialog = new DialogWindow(title, message, type)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        };
        dialog.ShowDialog();
        return dialog.Result;
    }
}
