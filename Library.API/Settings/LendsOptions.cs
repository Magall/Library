namespace Library.Settings;

public class LendsOptions
{
    public int MaxSimultaneousLends { get; set; }
    public int LendingDurationInWorkingDays { get; set; }
    public int BanishMultiplier { get; set; }
    public const string SectionName = "Lends";

}

