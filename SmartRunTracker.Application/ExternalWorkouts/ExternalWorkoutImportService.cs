using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.ExternalWorkouts.Analysis;
using SmartRunTracker.Application.ExternalWorkouts.Models;
using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Application.Workouts;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.ExternalWorkouts;

public class ExternalWorkoutImportService : IExternalWorkoutImportService
{
    private readonly IEnumerable<IExternalWorkoutFileParser> _fileParsers;
    private readonly IWorkoutRouteAnalyzer _routeAnalyzer;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ITrainingPlanRepository _trainingPlanRepository;

    public ExternalWorkoutImportService(
        IEnumerable<IExternalWorkoutFileParser> fileParsers,
        IWorkoutRouteAnalyzer routeAnalyzer,
        IWorkoutRepository workoutRepository,
        ITrainingPlanRepository trainingPlanRepository)
    {
        _fileParsers = fileParsers;
        _routeAnalyzer = routeAnalyzer;
        _workoutRepository = workoutRepository;
        _trainingPlanRepository = trainingPlanRepository;
    }

    public async Task<ExternalWorkoutImportResult> ImportGpxAsync(
        Stream fileStream,
        string fileName,
        ExternalWorkoutImportOptions options,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateImportOptions(options);

        if (validationError is not null)
        {
            return ExternalWorkoutImportResult.Failure(validationError);
        }

        var parser = _fileParsers.FirstOrDefault(x => x.Source == WorkoutSource.Gpx);

        if (parser is null)
        {
            return ExternalWorkoutImportResult.Failure("GPX parser is not registered.");
        }

        var parseResult = await parser.ParseAsync(
            fileStream,
            fileName,
            cancellationToken);

        if (!parseResult.IsSuccess || parseResult.Data is null)
        {
            return ExternalWorkoutImportResult.Failure(
                parseResult.ErrorMessage ?? "GPX file could not be parsed.");
        }

        var analysisResult = _routeAnalyzer.Analyze(parseResult.Data);

        if (!analysisResult.IsSuccess)
        {
            return ExternalWorkoutImportResult.Failure(
                analysisResult.ErrorMessage ?? "Workout route could not be analyzed.");
        }

        PlannedSession? plannedSession = null;

        if (options.PlannedSessionId is not null)
        {
            plannedSession = await _trainingPlanRepository.GetPlannedSessionByIdAsync(
                options.UserId,
                options.PlannedSessionId.Value,
                cancellationToken);

            if (plannedSession is null)
            {
                return ExternalWorkoutImportResult.Failure(
                    "Planned session was not found for this user.");
            }

            if (plannedSession.Status == PlannedSessionStatus.Completed)
            {
                return ExternalWorkoutImportResult.Failure(
                    "Planned session is already completed.");
            }
        }

        var workoutType = options.WorkoutType
            ?? plannedSession?.Type
            ?? WorkoutType.Easy;

        var workout = new Workout
        {
            UserId = options.UserId,
            PlannedSessionId = options.PlannedSessionId,
            StartedAt = analysisResult.StartedAt,
            DistanceKm = analysisResult.TotalDistanceKm,
            DurationSeconds = analysisResult.DurationSeconds,
            Rpe = options.Rpe!.Value,
            Type = workoutType,
            Source = parseResult.Data.Source,
            Notes = options.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            RoutePoints = analysisResult.RoutePoints
                .Select(ToWorkoutRoutePoint)
                .ToList(),
            Splits = analysisResult.Splits
                .Select(ToWorkoutSplit)
                .ToList()
        };

        var createdWorkout = await _workoutRepository.AddAsync(
            workout,
            cancellationToken);

        if (plannedSession is not null)
        {
            plannedSession.Status = PlannedSessionStatus.Completed;
            plannedSession.ScheduledFor ??= analysisResult.StartedAt;

            await _trainingPlanRepository.UpdatePlannedSessionAsync(
                plannedSession,
                cancellationToken);
        }

        return ExternalWorkoutImportResult.Success(ToWorkoutDto(createdWorkout));
    }

    private static string? ValidateImportOptions(ExternalWorkoutImportOptions options)
    {
        if (options is null)
        {
            return "Import options are missing.";
        }

        if (options.UserId <= 0)
        {
            return "User id is required.";
        }

        if (options.Rpe is null)
        {
            return "RPE is required for imported workouts.";
        }

        if (options.Rpe is < 1 or > 10)
        {
            return "RPE must be between 1 and 10.";
        }

        return null;
    }

    private static WorkoutRoutePoint ToWorkoutRoutePoint(
        AnalyzedRoutePoint point)
    {
        return new WorkoutRoutePoint
        {
            Order = point.Order,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            ElevationMeters = point.ElevationMeters,
            RecordedAt = point.RecordedAt,
            DistanceFromStartMeters = point.DistanceFromStartMeters,
            SecondsFromStart = point.SecondsFromStart,
            PaceSecondsPerKm = point.PaceSecondsPerKm
        };
    }

    private static WorkoutSplit ToWorkoutSplit(
        AnalyzedWorkoutSplit split)
    {
        return new WorkoutSplit
        {
            SplitNumber = split.SplitNumber,
            DistanceKm = split.DistanceKm,
            DurationSeconds = split.DurationSeconds,
            AveragePaceSecondsPerKm = split.AveragePaceSecondsPerKm,
            StartedAt = split.StartedAt,
            EndedAt = split.EndedAt
        };
    }

    private static WorkoutDto ToWorkoutDto(Workout workout)
    {
        var averagePaceSecondsPerKm = CalculateAveragePaceSecondsPerKm(workout);
        var sessionLoad = CalculateSessionLoad(workout);

        return new WorkoutDto(
            workout.Id,
            workout.UserId,
            workout.PlannedSessionId,
            workout.StartedAt,
            workout.DistanceKm,
            workout.DurationSeconds,
            averagePaceSecondsPerKm,
            workout.Rpe,
            sessionLoad,
            workout.Type,
            workout.Source,
            workout.Notes,
            workout.CreatedAt);
    }

    private static int CalculateAveragePaceSecondsPerKm(Workout workout)
    {
        if (workout.DistanceKm <= 0)
        {
            return 0;
        }

        return (int)Math.Round(
            (double)(workout.DurationSeconds / workout.DistanceKm),
            MidpointRounding.AwayFromZero);
    }

    private static int CalculateSessionLoad(Workout workout)
    {
        return (int)Math.Round(
            workout.DurationSeconds / 60.0 * workout.Rpe,
            MidpointRounding.AwayFromZero);
    }
}