using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.Workouts.Analysis;

public class WorkoutInsightBuilder : IWorkoutInsightBuilder
{
    public IReadOnlyList<WorkoutInsightDto> Build(
        Workout workout,
        PlannedVsActualDto? plannedVsActual)
    {
        var insights = new List<WorkoutInsightDto>();

        //AddSummaryInsight(workout, insights);
        //AddSourceInsight(workout, insights);
        //AddRouteInsight(workout, insights);
        AddSplitInsights(workout, insights);
        //AddPlannedVsActualInsight(plannedVsActual, insights);

        return insights;
    }

    private static void AddSummaryInsight(
        Workout workout,
        List<WorkoutInsightDto> insights)
    {
        var averagePace = CalculateAveragePaceSecondsPerKm(workout);

        insights.Add(new WorkoutInsightDto(
            "Workout summary",
            $"Completed {workout.DistanceKm} km in {FormatDuration(workout.DurationSeconds)} with average pace {FormatPace(averagePace)}.",
            WorkoutInsightSeverity.Info));
    }

    private static void AddSourceInsight(
        Workout workout,
        List<WorkoutInsightDto> insights)
    {
        if (workout.Source == WorkoutSource.Manual)
        {
            insights.Add(new WorkoutInsightDto(
                "Manual workout",
                "This workout was added manually, so route and split data may be unavailable.",
                WorkoutInsightSeverity.Info));

            return;
        }

        insights.Add(new WorkoutInsightDto(
            "Imported workout",
            $"This workout was imported from {workout.Source} and analyzed from external activity data.",
            WorkoutInsightSeverity.Positive));
    }

    private static void AddRouteInsight(
        Workout workout,
        List<WorkoutInsightDto> insights)
    {
        if (workout.RoutePoints.Count == 0)
        {
            insights.Add(new WorkoutInsightDto(
                "No route data",
                "Route visualization is unavailable for this workout.",
                WorkoutInsightSeverity.Info));

            return;
        }

        insights.Add(new WorkoutInsightDto(
            "Route data available",
            $"Route analysis uses {workout.RoutePoints.Count} GPS points.",
            WorkoutInsightSeverity.Positive));
    }

    private static void AddSplitInsights(
        Workout workout,
        List<WorkoutInsightDto> insights)
    {
        if (workout.Splits.Count == 0)
        {
            return;
        }
        var orderedSplits = workout.Splits
            .OrderBy(split => split.SplitNumber)
            .ToList();

        var fastestSplit = orderedSplits
            .OrderBy(split => split.AveragePaceSecondsPerKm)
            .First();

        var slowestSplit = orderedSplits
            .OrderByDescending(split => split.AveragePaceSecondsPerKm)
            .First();

        insights.Add(new WorkoutInsightDto(
            "Fastest split",
            $"Fastest split was km {fastestSplit.SplitNumber} at {FormatPace(fastestSplit.AveragePaceSecondsPerKm)}.",
            WorkoutInsightSeverity.Positive));

        insights.Add(new WorkoutInsightDto(
            "Slowest split",
            $"Slowest split was km {slowestSplit.SplitNumber} at {FormatPace(slowestSplit.AveragePaceSecondsPerKm)}.",
            WorkoutInsightSeverity.Info));

        var paceRangeSeconds =
            slowestSplit.AveragePaceSecondsPerKm -
            fastestSplit.AveragePaceSecondsPerKm;

        if (paceRangeSeconds <= 30)
        {
            insights.Add(new WorkoutInsightDto(
                "Stable pacing",
                "Split pace was relatively stable across the workout.",
                WorkoutInsightSeverity.Positive));
        }
        else if (paceRangeSeconds >= 90)
        {
            insights.Add(new WorkoutInsightDto(
                "Variable pacing",
                "Split pace changed significantly during the workout.",
                WorkoutInsightSeverity.Warning));
        }
    }

    private static void AddPlannedVsActualInsight(
        PlannedVsActualDto? plannedVsActual,
        List<WorkoutInsightDto> insights)
    {
        if (plannedVsActual is null)
        {
            insights.Add(new WorkoutInsightDto(
                "No planned comparison",
                "This workout is not linked to a planned session, so planned-vs-actual analysis is unavailable.",
                WorkoutInsightSeverity.Info));

            return;
        }

        var severity = plannedVsActual.Status switch
        {
            PlannedVsActualStatus.Matched => WorkoutInsightSeverity.Positive,
            PlannedVsActualStatus.PartiallyMatched => WorkoutInsightSeverity.Info,
            PlannedVsActualStatus.NotMatched => WorkoutInsightSeverity.Warning,
            _ => WorkoutInsightSeverity.Info
        };

        insights.Add(new WorkoutInsightDto(
            "Planned vs actual",
            plannedVsActual.Messages.FirstOrDefault()
                ?? "Workout was compared with the planned session.",
            severity));
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

    private static string FormatDuration(int totalSeconds)
    {
        var time = TimeSpan.FromSeconds(totalSeconds);

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours}h {time.Minutes}m {time.Seconds}s";
        }

        return $"{time.Minutes}m {time.Seconds}s";
    }

    private static string FormatPace(int secondsPerKm)
    {
        if (secondsPerKm <= 0)
        {
            return "-";
        }

        var minutes = secondsPerKm / 60;
        var seconds = secondsPerKm % 60;

        return $"{minutes}:{seconds:00}/km";
    }
}