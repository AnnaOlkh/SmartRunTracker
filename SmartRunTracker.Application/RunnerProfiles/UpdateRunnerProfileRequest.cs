using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.RunnerProfiles;

public sealed record UpdateRunnerProfileRequest(
    int PreferredWorkoutsPerWeek,
    TrainingDayPreferenceMode TrainingDayPreferenceMode,
    IReadOnlyList<TrainingDay> AvailableDays
);