using System.Collections.ObjectModel;
using FitnessClubIS.Data;
using FitnessClubIS.Models;
using FitnessClubIS.Services;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubIS.ViewModels;

public class ScheduleViewModel : ViewModelBase
{
    public ObservableCollection<TrainingSession> Sessions { get; } = new();

    private DateTime _date = DateTime.Today;
    public DateTime Date
    {
        get => _date;
        set { if (Set(ref _date, value)) Load(); }
    }

    public ScheduleViewModel()
    {
        Title = "Расписание занятий";
        Load();
    }

    private void Load()
    {
        try
        {
            using var db = new FitnessContext();
            var from = Date.Date;
            var to = from.AddDays(7);
            var list = db.Sessions
                .Include(s => s.WorkoutType).Include(s => s.Trainer).Include(s => s.Bookings)
                .AsNoTracking()
                .Where(s => s.StartsAt >= from && s.StartsAt < to)
                .OrderBy(s => s.StartsAt).ToList();
            Sessions.Clear();
            foreach (var s in list) Sessions.Add(s);
        }
        catch (Exception ex)
        {
            AppServices.Instance.Get<IDialogService>()
                .ShowDialog("Ошибка", "Не удалось загрузить расписание:\n" + ex.Message, DialogType.Error);
        }
    }
}
