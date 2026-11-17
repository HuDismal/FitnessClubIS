using FitnessClubIS.Data;
using Microsoft.EntityFrameworkCore;
using Word = Microsoft.Office.Interop.Word;

namespace FitnessClubIS.Services;

/// <summary>Программное формирование отчёта о платежах в виде документа Microsoft Word.</summary>
public class WordReportService
{
    /// <summary>
    /// Формирует отчёт в отдельном потоке (Task.Run). Прогресс сообщается через callback,
    /// который вызывающая сторона выполняет в основном потоке через Dispatcher.
    /// </summary>
    public Task CreateAsync(DateTime from, DateTime to, string path, Action<string> progress)
        => Task.Run(() =>
        {
            progress("Чтение данных из базы…");
            using var db = new FitnessContext();
            var end = to.Date.AddDays(1);
            var payments = db.Payments
                .Include(p => p.ClientMembership).ThenInclude(m => m.Client)
                .Include(p => p.ClientMembership).ThenInclude(m => m.MembershipType)
                .AsNoTracking()
                .Where(p => p.PaidAt >= from.Date && p.PaidAt < end)
                .OrderBy(p => p.PaidAt).ToList();

            progress("Запуск Microsoft Word…");
            var app = new Word.Application { Visible = false };
            Word.Document doc = null;
            try
            {
                doc = app.Documents.Add();
                var content = doc.Content;
                content.Font.Name = "Times New Roman";
                content.Font.Size = 14;

                AddParagraph(doc, "Фитнес-клуб «Атлет»", 18, true, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                AddParagraph(doc, "Отчёт о платежах", 16, true, Word.WdParagraphAlignment.wdAlignParagraphCenter);
                AddParagraph(doc, $"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}", 14, false, Word.WdParagraphAlignment.wdAlignParagraphCenter);

                // ---- таблица по месяцам ----
                var byMonth = payments.GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
                                      .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ToList();
                AddParagraph(doc, "Выручка по месяцам", 14, true, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                var t1 = AddTable(doc, byMonth.Count + 2, 3);
                SetRow(t1, 1, "Месяц", "Платежей", "Сумма, ₽");
                for (int i = 0; i < byMonth.Count; i++)
                {
                    var g = byMonth[i];
                    SetRow(t1, i + 2, $"{g.Key.Month:00}.{g.Key.Year}", g.Count().ToString(), g.Sum(x => x.Amount).ToString("N2"));
                }
                SetRow(t1, byMonth.Count + 2, "Итого", payments.Count.ToString(), payments.Sum(x => x.Amount).ToString("N2"));
                progress("Таблица по месяцам готова");

                // ---- подробная таблица ----
                AddParagraph(doc, "Список платежей", 14, true, Word.WdParagraphAlignment.wdAlignParagraphLeft);
                var t2 = AddTable(doc, payments.Count + 1, 5);
                SetRow(t2, 1, "Дата", "Клиент", "Абонемент", "Способ", "Сумма, ₽");
                for (int i = 0; i < payments.Count; i++)
                {
                    var p = payments[i];
                    SetRow(t2, i + 2, p.PaidAt.ToString("dd.MM.yyyy HH:mm"), p.ClientMembership.Client.FullName,
                           p.ClientMembership.MembershipType.Name, p.Method, p.Amount.ToString("N2"));
                }

                progress("Сохранение документа…");
                doc.SaveAs2(path);
                progress("Отчёт сохранён");
            }
            finally
            {
                doc?.Close(false);
                app.Quit(false);
            }
        });

    private static void AddParagraph(Word.Document doc, string text, int size, bool bold, Word.WdParagraphAlignment align)
    {
        var range = doc.Content;
        range.InsertParagraphAfter();
        var p = doc.Paragraphs[doc.Paragraphs.Count];
        p.Range.Text = text;
        p.Range.Font.Size = size;
        p.Range.Font.Bold = bold ? 1 : 0;
        p.Alignment = align;
        p.Range.InsertParagraphAfter();
    }

    private static Word.Table AddTable(Word.Document doc, int rows, int cols)
    {
        var range = doc.Paragraphs[doc.Paragraphs.Count].Range;
        var table = doc.Tables.Add(range, rows, cols);
        table.Borders.Enable = 1;
        table.Range.Font.Size = 11;
        table.Range.Font.Bold = 0;
        table.Rows[1].Range.Font.Bold = 1;
        return table;
    }

    private static void SetRow(Word.Table table, int row, params string[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
            table.Cell(row, i + 1).Range.Text = cells[i];
    }
}
