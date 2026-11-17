using System.Windows;
using System.Windows.Controls;

namespace FitnessClubIS.Views.Controls;

/// <summary>Пользовательский элемент: подпись + поле ввода. Label и Text — свойства зависимости (Dependency Property).</summary>
public partial class LabeledTextBox : UserControl
{
    public LabeledTextBox() => InitializeComponent();

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledTextBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(LabeledTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
}
