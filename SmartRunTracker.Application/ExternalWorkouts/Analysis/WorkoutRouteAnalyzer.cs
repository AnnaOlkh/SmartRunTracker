using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.ExternalWorkouts.Models;

namespace SmartRunTracker.Application.ExternalWorkouts.Analysis;

public class WorkoutRouteAnalyzer : IWorkoutRouteAnalyzer
{
    private const double EarthRadiusMeters = 6371000;
    private const decimal MinimumSplitDistanceKm = 0.01m;

    public WorkoutRouteAnalysisResult Analyze(ExternalWorkoutData workoutData)
    {
        if (workoutData is null)
        {
            return WorkoutRouteAnalysisResult.Failure("Workout data is missing.");
        }

        if (workoutData.Points.Count < 2)
        {
            return WorkoutRouteAnalysisResult.Failure("At least two route points are required for route analysis.");
        }

        var points = workoutData.Points
            .OrderBy(x => x.Order)
            .ToList();

        foreach (var point in points)
        {
            if (!IsValidLatitude(point.Latitude))
            {
                return WorkoutRouteAnalysisResult.Failure($"Invalid latitude in route point #{point.Order}.");
            }

            if (!IsValidLongitude(point.Longitude))
            {
                return WorkoutRouteAnalysisResult.Failure($"Invalid longitude in route point #{point.Order}.");
            }

            if (!point.RecordedAt.HasValue)
            {
                return WorkoutRouteAnalysisResult.Failure($"Missing timestamp in route point #{point.Order}.");
            }
        }

        var startedAt = points.First().RecordedAt!.Value;
        var endedAt = points.Last().RecordedAt!.Value;

        if (endedAt <= startedAt)
        {
            return WorkoutRouteAnalysisResult.Failure("Workout end time must be later than start time.");
        }

        var analyzedPoints = new List<AnalyzedRoutePoint>();

        decimal totalDistanceMeters = 0;

        analyzedPoints.Add(new AnalyzedRoutePoint
        {
            Order = points[0].Order,
            Latitude = points[0].Latitude,
            Longitude = points[0].Longitude,
            ElevationMeters = points[0].ElevationMeters,
            RecordedAt = points[0].RecordedAt,
            DistanceFromStartMeters = 0,
            SecondsFromStart = 0,
            PaceSecondsPerKm = null
        });

        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1];
            var current = points[i];

            if (current.RecordedAt!.Value < previous.RecordedAt!.Value)
            {
                return WorkoutRouteAnalysisResult.Failure($"Route point #{current.Order} has an earlier timestamp than the previous point.");
            }

            var segmentDistanceMeters = CalculateDistanceMeters(
                previous.Latitude,
                previous.Longitude,
                current.Latitude,
                current.Longitude);

            totalDistanceMeters += segmentDistanceMeters;

            var segmentDurationSeconds = (int)Math.Round(
                (current.RecordedAt.Value - previous.RecordedAt.Value).TotalSeconds);

            var secondsFromStart = (int)Math.Round(
                (current.RecordedAt.Value - startedAt).TotalSeconds);

            int? segmentPaceSecondsPerKm = null;

            if (segmentDistanceMeters > 0 && segmentDurationSeconds > 0)
            {
                segmentPaceSecondsPerKm = (int)Math.Round(
                    segmentDurationSeconds / ((double)segmentDistanceMeters / 1000));
            }

            analyzedPoints.Add(new AnalyzedRoutePoint
            {
                Order = current.Order,
                Latitude = current.Latitude,
                Longitude = current.Longitude,
                ElevationMeters = current.ElevationMeters,
                RecordedAt = current.RecordedAt,
                DistanceFromStartMeters = totalDistanceMeters,
                SecondsFromStart = secondsFromStart,
                PaceSecondsPerKm = segmentPaceSecondsPerKm
            });
        }

        if (totalDistanceMeters <= 0)
        {
            return WorkoutRouteAnalysisResult.Failure("Route distance must be greater than zero.");
        }

        var durationSeconds = (int)Math.Round((endedAt - startedAt).TotalSeconds);

        if (durationSeconds <= 0)
        {
            return WorkoutRouteAnalysisResult.Failure("Workout duration must be greater than zero.");
        }

        var totalDistanceKm = decimal.Round(totalDistanceMeters / 1000, 3);

        var averagePaceSecondsPerKm = (int)Math.Round(
            durationSeconds / (double)totalDistanceKm);

        var splits = BuildSplits(analyzedPoints, totalDistanceMeters);

        return WorkoutRouteAnalysisResult.Success(
            startedAt,
            endedAt,
            totalDistanceKm,
            durationSeconds,
            averagePaceSecondsPerKm,
            analyzedPoints,
            splits);
    }

    private static List<AnalyzedWorkoutSplit> BuildSplits(
        List<AnalyzedRoutePoint> points,
        decimal totalDistanceMeters)
    {
        var splits = new List<AnalyzedWorkoutSplit>();

        var splitNumber = 1;
        decimal splitStartDistanceMeters = 0;
        var splitStartTime = points.First().RecordedAt!.Value;

        while (splitStartDistanceMeters < totalDistanceMeters)
        {
            var splitEndDistanceMeters = Math.Min(splitNumber * 1000m, totalDistanceMeters);
            var splitDistanceKm = decimal.Round(
                (splitEndDistanceMeters - splitStartDistanceMeters) / 1000,
                3);

            if (splitDistanceKm < MinimumSplitDistanceKm)
            {
                break;
            }

            var splitEndTime = GetTimeAtDistance(points, splitEndDistanceMeters);
            var splitDurationSeconds = (int)Math.Round((splitEndTime - splitStartTime).TotalSeconds);

            if (splitDurationSeconds > 0)
            {
                var averagePaceSecondsPerKm = (int)Math.Round(
                    splitDurationSeconds / (double)splitDistanceKm);

                splits.Add(new AnalyzedWorkoutSplit
                {
                    SplitNumber = splitNumber,
                    DistanceKm = splitDistanceKm,
                    DurationSeconds = splitDurationSeconds,
                    AveragePaceSecondsPerKm = averagePaceSecondsPerKm,
                    StartedAt = splitStartTime,
                    EndedAt = splitEndTime
                });
            }

            splitStartDistanceMeters = splitEndDistanceMeters;
            splitStartTime = splitEndTime;
            splitNumber++;
        }

        return splits;
    }

    private static DateTimeOffset GetTimeAtDistance(
        List<AnalyzedRoutePoint> points,
        decimal targetDistanceMeters)
    {
        if (targetDistanceMeters <= 0)
        {
            return points.First().RecordedAt!.Value;
        }

        if (targetDistanceMeters >= points.Last().DistanceFromStartMeters)
        {
            return points.Last().RecordedAt!.Value;
        }

        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1];
            var current = points[i];

            if (current.DistanceFromStartMeters < targetDistanceMeters)
            {
                continue;
            }

            var segmentDistanceMeters =
                current.DistanceFromStartMeters - previous.DistanceFromStartMeters;

            if (segmentDistanceMeters <= 0)
            {
                return current.RecordedAt!.Value;
            }

            var distanceIntoSegment =
                targetDistanceMeters - previous.DistanceFromStartMeters;

            var ratio = (double)(distanceIntoSegment / segmentDistanceMeters);

            var segmentDurationSeconds =
                (current.RecordedAt!.Value - previous.RecordedAt!.Value).TotalSeconds;

            return previous.RecordedAt.Value.AddSeconds(segmentDurationSeconds * ratio);
        }

        return points.Last().RecordedAt!.Value;
    }

    private static decimal CalculateDistanceMeters(
        decimal latitude1,
        decimal longitude1,
        decimal latitude2,
        decimal longitude2)
    {
        var lat1 = ToRadians((double)latitude1);
        var lon1 = ToRadians((double)longitude1);
        var lat2 = ToRadians((double)latitude2);
        var lon2 = ToRadians((double)longitude2);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a =
            Math.Pow(Math.Sin(deltaLat / 2), 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return (decimal)(EarthRadiusMeters * c);
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private static bool IsValidLatitude(decimal latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    private static bool IsValidLongitude(decimal longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }
}