namespace SmartRunTracker.Application.Workouts.Analysis;

public enum PlannedVsActualMetricStatus
{
    Matched = 1,
    LowerThanPlanned = 2,
    HigherThanPlanned = 3,
    FasterThanPlanned = 4,
    SlowerThanPlanned = 5,
    NotAvailable = 6
}