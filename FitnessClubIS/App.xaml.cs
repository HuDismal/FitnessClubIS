using System.Windows;
using FitnessClubIS.Services;

namespace FitnessClubIS;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // регистрация единственного экземпляра службы диалоговых окон
        AppServices.Instance.Add<IDialogService>(new DialogService());

    }
    static App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}
