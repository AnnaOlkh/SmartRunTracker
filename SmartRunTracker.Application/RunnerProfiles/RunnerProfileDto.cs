using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.RunnerProfiles;

public sealed record RunnerProfileDto(
    int Id,
    int UserId,
    int PreferredWorkoutsPerWeek,
    TrainingDayPreferenceMode TrainingDayPreferenceMode,
    IReadOnlyList<TrainingDay> AvailableDays,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);