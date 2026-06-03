using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts.Analysis;

public class PlannedVsActualAnalyzer : IPlannedVsActualAnalyzer
{
    private const decimal DurationTolerancePercent = 10m;
    private const decimal DistanceTolerancePercent = 10m;
    private const decimal PaceTolerancePercent = 8m;

    public PlannedVsActualDto? Analyze(Workout workout)
    {
        if (workout.PlannedSession is null)
        {
            return null;
        }

        var plannedSession = workout.PlannedSession;

        var duration = CompareDuration(
            plannedSession.TargetDurationSeconds,
            workout.DurationSeconds);

        var distance = plannedSession.TargetDistanceKm is null
            ? null
            : CompareDistance(
                plannedSession.TargetDistanceKm.Value,
                workout.DistanceKm);

        var actualPaceSecondsPerKm = CalculateAveragePaceSecondsPerKm(workout);

        var pace = plannedSession.TargetPaceSecondsPerKm is null
            ? null
            : ComparePace(
                plannedSession.TargetPaceSecondsPerKm.Value,
                actualPaceSecondsPerKm);

        var type = CompareType(
            plannedSession.Type,
            workout.Type);

        var intensity = CompareIntensity(
            plannedSession.Intensity,
            workout.Rpe);

        var checks = new List<bool>
        {
            duration.Status == PlannedVsActualMetricStatus.Matched,
            type.IsMatched,
            intensity.IsMatched
        };

        if (distance is not null)
        {
            checks.Add(distance.Status == PlannedVsActualMetricStatus.Matched);
        }

        if (pace is not null)
        {
            checks.Add(pace.Status == PlannedVsActualMetricStatus.Matched);
        }

        var matchedChecks = checks.Count(x => x);
        var status = DetermineStatus(matchedChecks, checks.Count);

        var messages = BuildMessages(
            status,
            duration,
            distance,
            pace,
            type,
            intensity);

        return new PlannedVsActualDto(
            status,
            messages,
            duration,
            distance,
            pace,
            type,
            intensity);
    }

    private static PlannedVsActualMetricDto CompareDuration(
        int plannedDurationSeconds,
        int actualDurationSeconds)
    {
        var difference = actualDurationSeconds - plannedDurationSeconds;
        var differencePercent = CalculateDifferencePercent(
            plannedDurationSeconds,
            actualDurationSeconds);

        var status = Math.Abs(differencePercent) <= DurationTolerancePercent
            ? PlannedVsActualMetricStatus.Matched
            : difference > 0
                ? PlannedVsActualMetricStatus.HigherThanPlanned
                : PlannedVsActualMetricStatus.LowerThanPlanned;

        var message = status switch
        {
            PlannedVsActualMetricStatus.Matched =>
                "Workout duration matched the plan.",
            PlannedVsActualMetricStatus.HigherThanPlanned =>
                "Workout was longer than planned.",
            PlannedVsActualMetricStatus.LowerThanPlanned =>
                "Workout was shorter than planned.",
            _ => "Workout duration could not be compared."
        };

        return new PlannedVsActualMetricDto(
            "Duration",
            plannedDurationSeconds,
            actualDurationSeconds,
            difference,
            differencePercent,
            "seconds",
            status,
            message);
    }

    private static PlannedVsActualMetricDto CompareDistance(
        decimal plannedDistanceKm,
        decimal actualDistanceKm)
    {
        var difference = actualDistanceKm - plannedDistanceKm;
        var differencePercent = CalculateDifferencePercent(
            plannedDistanceKm,
            actualDistanceKm);

        var status = Math.Abs(differencePercent) <= DistanceTolerancePercent
            ? PlannedVsActualMetricStatus.Matched
            : difference > 0
                ? PlannedVsActualMetricStatus.HigherThanPlanned
                : PlannedVsActualMetricStatus.LowerThanPlanned;

        var message = status switch
        {
            PlannedVsActualMetricStatus.Matched =>
                "Workout distance matched the plan.",
            PlannedVsActualMetricStatus.HigherThanPlanned =>
                "Workout was longer than the planned distance.",
            PlannedVsActualMetricStatus.LowerThanPlanned =>
                "Workout was shorter than the planned distance.",
            _ => "Workout distance could not be compared."
        };

        return new PlannedVsActualMetricDto(
            "Distance",
            plannedDistanceKm,
            actualDistanceKm,
            difference,
            differencePercent,
            "km",
            status,
            message);
    }

    private static PlannedVsActualMetricDto ComparePace(
        int plannedPaceSecondsPerKm,
        int actualPaceSecondsPerKm)
    {
        var difference = actualPaceSecondsPerKm - plannedPaceSecondsPerKm;
        var differencePercent = CalculateDifferencePercent(
            plannedPaceSecondsPerKm,
            actualPaceSecondsPerKm);

        var status = Math.Abs(differencePercent) <= PaceTolerancePercent
            ? PlannedVsActualMetricStatus.Matched
            : difference > 0
                ? PlannedVsActualMetricStatus.SlowerThanPlanned
                : PlannedVsActualMetricStatus.FasterThanPlanned;

        var message = status switch
        {
            PlannedVsActualMetricStatus.Matched =>
                "Workout pace matched the plan.",
            PlannedVsActualMetricStatus.SlowerThanPlanned =>
                "Workout was slower than planned.",
            PlannedVsActualMetricStatus.FasterThanPlanned =>
                "Workout was faster than planned.",
            _ => "Workout pace could not be compared."
        };

        return new PlannedVsActualMetricDto(
            "Pace",
            plannedPaceSecondsPerKm,
            actualPaceSecondsPerKm,
            difference,
            differencePercent,
            "seconds_per_km",
            status,
            message);
    }

    private static PlannedVsActualTypeDto CompareType(
        WorkoutType plannedType,
        WorkoutType actualType)
    {
        var isMatched = plannedType == actualType;

        return new PlannedVsActualTypeDto(
            plannedType,
            actualType,
            isMatched,
            isMatched
                ? "Workout type matched the plan."
                : "Workout type differed from the planned session.");
    }

    private static PlannedVsActualIntensityDto CompareIntensity(
        WorkoutIntensity plannedIntensity,
        int actualRpe)
    {
        var isMatched = plannedIntensity switch
        {
            WorkoutIntensity.Low => actualRpe is >= 1 and <= 4,
            WorkoutIntensity.Moderate => actualRpe is >= 5 and <= 7,
            WorkoutIntensity.High => actualRpe is >= 8 and <= 10,
            _ => false
        };

        var message = isMatched
            ? "Workout effort matched the planned intensity."
            : "Workout effort differed from the planned intensity.";

        return new PlannedVsActualIntensityDto(
            plannedIntensity,
            actualRpe,
            isMatched,
            message);
    }

    private static PlannedVsActualStatus DetermineStatus(
        int matchedChecks,
        int totalChecks)
    {
        if (matchedChecks == totalChecks)
        {
            return PlannedVsActualStatus.Matched;
        }

        if (matchedChecks >= Math.Ceiling(totalChecks / 2.0))
        {
            return PlannedVsActualStatus.PartiallyMatched;
        }

        return PlannedVsActualStatus.NotMatched;
    }

    private static List<string> BuildMessages(
        PlannedVsActualStatus status,
        PlannedVsActualMetricDto duration,
        PlannedVsActualMetricDto? distance,
        PlannedVsActualMetricDto? pace,
        PlannedVsActualTypeDto type,
        PlannedVsActualIntensityDto intensity)
    {
        var messages = new List<string>
        {
            status switch
            {
                PlannedVsActualStatus.Matched =>
                    "Workout matched the planned session.",
                PlannedVsActualStatus.PartiallyMatched =>
                    "Workout partially matched the planned session.",
                PlannedVsActualStatus.NotMatched =>
                    "Workout did not match the planned session.",
                _ =>
                    "Workout was compared with the planned session."
            },
            duration.Message,
            type.Message,
            intensity.Message
        };

        if (distance is not null)
        {
            messages.Add(distance.Message);
        }

        if (pace is not null)
        {
            messages.Add(pace.Message);
        }

        return messages;
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

    private static decimal CalculateDifferencePercent(
        decimal plannedValue,
        decimal actualValue)
    {
        if (plannedValue == 0)
        {
            return 0;
        }

        return decimal.Round(
            (actualValue - plannedValue) / plannedValue * 100,
            2);
    }

    private static decimal CalculateDifferencePercent(
        int plannedValue,
        int actualValue)
    {
        if (plannedValue == 0)
        {
            return 0;
        }

        return decimal.Round(
            ((decimal)actualValue - plannedValue) / plannedValue * 100,
            2);
    }
}