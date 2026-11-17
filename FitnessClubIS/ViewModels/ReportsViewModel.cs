using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FitnessClubIS.Data;
using FitnessClubIS.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Win32;

namespace FitnessClubIS.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly IDialogService _dialog = AppServices.Instance.Get<IDialogService>();

    public ReportsViewModel()
    {
        Title = "Отчёты";
        _dateTo = DateTime.Today;
        _dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !IsBusy);
        Refresh();
    }

    // ---------- период (дополнительное задание) ----------
    private DateTime _dateFrom;
    public DateTime DateFrom { get => _dateFrom; set { if (Set(ref _dateFrom, value)) Refresh(); } }

    private DateTime _dateTo;
    public DateTime DateTo { get => _dateTo; set { if (Set(ref _dateTo, value)) Refresh(); } }

    // ---------- данные графиков ----------
    private ISeries[] _revenueSeries = Array.Empty<ISeries>();
    public ISeries[] RevenueSeries { get => _revenueSeries; private set => Set(ref _revenueSeries, value); }

    private Axis[] _xAxes = Array.Empty<Axis>();
    public Axis[] XAxes { get => _xAxes; private set => Set(ref _xAxes, value); }

    private ISeries[] _visitsSeries = Array.Empty<ISeries>();
    public ISeries[] VisitsSeries { get => _visitsSeries; private set => Set(ref _visitsSeries, value); }

    private string _total;
    public string Total { get => _total; private set => Set(ref _total, value); }

    // ---------- экспорт в Word ----------
    private bool _isBusy;
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) ExportCommand.NotifyCanExecuteChanged(); } }

    private string _status;
    public string Status { get => _status; private set => Set(ref _status, value); }

    public AsyncRelayCommand ExportCommand { get; }

    private void Refresh()
    {
        if (DateFrom > DateTo) { Status = "Дата начала позже даты окончания"; return; }
        Status = null;
        try
        {
            using var db = new FitnessContext();
            var from = DateFrom.Date;
            var end = DateTo.Date.AddDays(1);

            // выручка по месяцам
            var rev = db.Payments.Where(p => p.PaidAt >= from && p.PaidAt < end)
                .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();

            RevenueSeries = new ISeries[]
            {
                new ColumnSeries<double> { Name = "Выручка, ₽", Values = rev.Select(x => (double)x.Sum).ToArray() }
            };
            XAxes = new[] { new Axis { Labels = rev.Select(x => $"{x.Month:00}.{x.Year}").ToArray() } };
            Total = $"Итого за период: {rev.Sum(x => x.Sum):N2} ₽";

            // посещаемость по направлениям
            var visits = db.Bookings
                .Where(b => b.Attended == true)
                .Join(db.Sessions, b => b.SessionId, s => s.Id, (b, s) => s)
                .Where(s => s.StartsAt >= from && s.StartsAt < end)
                .GroupBy(s => s.WorkoutType.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToList();
            VisitsSeries = visits
                .Select(v => (ISeries)new PieSeries<double> { Name = v.Name, Values = new[] { (double)v.Count } })
                .ToArray();
        }
        catch (Exception ex)
        {
            _dialog.ShowDialog("Ошибка", "Не удалось построить графики:\n" + ex.Message, DialogType.Error);
        }
    }

    private async Task ExportAsync()
    {
        var dlg = new SaveFileDialog { Filter = "Документ Word (*.docx)|*.docx", FileName = $"Отчёт_{DateFrom:yyyyMMdd}_{DateTo:yyyyMMdd}.docx" };
        if (dlg.ShowDialog() != true) return;

        IsBusy = true;
        try
        {
            // callback выполняется в потоке формирования отчёта, поэтому обращаемся к UI через Dispatcher
            await new WordReportService().CreateAsync(DateFrom, DateTo, dlg.FileName,
                msg => Application.Current.Dispatcher.Invoke(() => Status = msg));
            _dialog.ShowDialog("Готово", "Отчёт сформирован:\n" + dlg.FileName, DialogType.Info);
        }
        catch (Exception ex)
        {
            _dialog.ShowDialog("Ошибка", "Не удалось сформировать отчёт (установлен ли Microsoft Word?):\n" + ex.Message, DialogType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
