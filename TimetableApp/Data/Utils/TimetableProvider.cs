using TimetableApp.Data.Database;
using TimetableApp.Utils;

namespace TimetableApp.Data.Utils;

public class TimetableProvider : ITimetableProvider
{
    private static TimetableDbContext db;

    public Task<IEnumerable<Timetable>> GetTimetablesAsync(DateOnly dateStart, DateOnly dateEnd)
    {
        var timetables = db.Timetables.Select(item => item).ToArray();
        var getTimetable = db.Timetables.Where(item => new DateTime(dateStart.Year, dateStart.Month, dateStart.Day) <= item.Date && item.Date <= new DateTime(dateEnd.Year, dateEnd.Month, dateEnd.Day) && TimetablesConflictDetector.ConflictExists(item, timetables));

        return Task.FromResult<IEnumerable<Timetable>>(getTimetable);
    }

    public Task<Timetable> CreateTimetableAsync(TimetableBuilder timetableBuilder)
    {
        var timetables = db.Timetables.Select(item => item).ToArray();
        var timetableExemplar = timetableBuilder.Build();

        if (TimetablesConflictDetector.ConflictExists(timetableExemplar, timetables))
        {
            db.Timetables.Add(timetableExemplar);
            db.SaveChanges();
        }

        return Task.FromResult(timetableExemplar);
    }

    public Task<Timetable> EditTimetableAsync(long timetableId, TimetableEditor editor)
    {
        var timetables = db.Timetables.Select(item => item).ToArray();
        var timetableExemplar = db.Timetables.FirstOrDefault(item => item.Id == timetableId && TimetablesConflictDetector.ConflictExists(item, timetables));

        editor.Edit(timetableExemplar);
        db.Timetables.Update(timetableExemplar);
        db.SaveChangesAsync();

        return Task.FromResult(timetableExemplar);
    }

    public Task DeleteTimetableAsync(long timetableId)
    {
        var TimetableExemplar = db.Timetables.FirstOrDefault(item => item.Id == timetableId);
        
        db.Timetables.Remove(TimetableExemplar);
        db.SaveChangesAsync();

        return Task.FromResult(TimetableExemplar);
    }
}