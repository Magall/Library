namespace Library.Utils;

public static class DateExtensions
{

    public static DateTime AddWorkingDays(DateTime start, int days)
    {
        var date = start;
        var added = 0;

        while (added < days)
        {
            date = date.AddDays(1);

            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                added++;
            }
        }

        return date;
    }
}
