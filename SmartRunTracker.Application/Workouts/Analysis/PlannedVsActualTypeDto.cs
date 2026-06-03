using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts.Analysis;

public sealed record PlannedVsActualTypeDto(
    WorkoutType PlannedType,
    WorkoutType ActualType,
    bool IsMatched,
    string Message);