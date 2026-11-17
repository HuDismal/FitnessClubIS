using System.Windows;
using System.Windows.Controls;
using FitnessClubIS.Services;

namespace FitnessClubIS.Views;

public partial class DialogWindow : Window
{
    public DialogResultType Result { get; private set; } = DialogResultType.No;

    public DialogWindow(string title, string message, DialogType type)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        IconText.Text = type switch
        {
            DialogType.Info => "ℹ",
            DialogType.Warning => "⚠",
            DialogType.Error => "⛔",
            _ => "❓"
        };

        if (type == DialogType.Question)
        {
            ButtonsPanel.Children.Add(MakeButton("Да", (_, _) => Close(DialogResultType.Yes), false));
            ButtonsPanel.Children.Add(MakeButton("Нет", (_, _) => Close(DialogResultType.No), true));
        }
        else
        {
            ButtonsPanel.Children.Add(MakeButton("OK", (_, _) => Close(DialogResultType.Ok), false));
        }
    }

    private Button MakeButton(string text, RoutedEventHandler click, bool secondary)
    {
        var b = new Button { Content = text, Margin = new Thickness(6, 0, 6, 0), Width = 110 };
        if (secondary) b.Style = (Style)FindResource("DangerButton");
        b.Click += click;
        return b;
    }

    private void Close(DialogResultType r) { Result = r; Close(); }
}
