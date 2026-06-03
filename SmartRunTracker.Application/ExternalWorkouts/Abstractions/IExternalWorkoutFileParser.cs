using SmartRunTracker.Application.ExternalWorkouts.Models;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.ExternalWorkouts.Abstractions;

public interface IExternalWorkoutFileParser
{
    WorkoutSource Source { get; }

    Task<ExternalWorkoutParseResult> ParseAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}