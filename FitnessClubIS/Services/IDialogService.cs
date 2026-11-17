namespace FitnessClubIS.Services;

public enum DialogType { Info, Warning, Error, Question }
public enum DialogResultType { Ok, Yes, No }

public interface IDialogService
{
    DialogResultType ShowDialog(string title, string message, DialogType type);
}
