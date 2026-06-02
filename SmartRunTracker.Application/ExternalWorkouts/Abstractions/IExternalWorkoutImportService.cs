using SmartRunTracker.Application.ExternalWorkouts.Models;

namespace SmartRunTracker.Application.ExternalWorkouts.Abstractions;

public interface IExternalWorkoutImportService
{
    Task<ExternalWorkoutImportResult> ImportGpxAsync(
        Stream fileStream,
        string fileName,
        ExternalWorkoutImportOptions options,
        CancellationToken cancellationToken = default);
}