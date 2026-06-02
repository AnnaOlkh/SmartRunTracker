using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts.Analysis;

public sealed record PlannedVsActualIntensityDto(
    WorkoutIntensity PlannedIntensity,
    int ActualRpe,
    bool IsMatched,
    string Message);