using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.ExternalWorkouts.Models;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Infrastructure.ExternalWorkouts.Gpx;

public class GpxWorkoutFileParser : IExternalWorkoutFileParser
{
    public WorkoutSource Source => WorkoutSource.Gpx;

    public async Task<ExternalWorkoutParseResult> ParseAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
        {
            return ExternalWorkoutParseResult.Failure("GPX file stream is missing.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ExternalWorkoutParseResult.Failure("GPX file name is missing.");
        }

        if (!fileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase))
        {
            return ExternalWorkoutParseResult.Failure("Only .gpx files are supported.");
        }

        if (fileStream.CanSeek)
        {
            if (fileStream.Length == 0)
            {
                return ExternalWorkoutParseResult.Failure("GPX file is empty.");
            }

            fileStream.Position = 0;
        }

        XDocument document;

        try
        {
            document = await XDocument.LoadAsync(
                fileStream,
                LoadOptions.None,
                cancellationToken);
        }
        catch (XmlException)
        {
            return ExternalWorkoutParseResult.Failure("GPX file is not a valid XML document.");
        }
        catch (InvalidOperationException)
        {
            return ExternalWorkoutParseResult.Failure("GPX file could not be read.");
        }

        var trackPoints = document
            .Descendants()
            .Where(x => x.Name.LocalName == "trkpt")
            .ToList();

        if (trackPoints.Count < 2)
        {
            return ExternalWorkoutParseResult.Failure("GPX file must contain at least two track points.");
        }

        var points = new List<ExternalWorkoutPoint>();

        for (var i = 0; i < trackPoints.Count; i++)
        {
            var trackPoint = trackPoints[i];

            var latitudeValue = trackPoint.Attribute("lat")?.Value;
            var longitudeValue = trackPoint.Attribute("lon")?.Value;

            if (!TryParseDecimal(latitudeValue, out var latitude))
            {
                return ExternalWorkoutParseResult.Failure($"Invalid latitude in GPX point #{i + 1}.");
            }

            if (!TryParseDecimal(longitudeValue, out var longitude))
            {
                return ExternalWorkoutParseResult.Failure($"Invalid longitude in GPX point #{i + 1}.");
            }

            var elevationText = GetChildValue(trackPoint, "ele");
            decimal? elevationMeters = null;

            if (!string.IsNullOrWhiteSpace(elevationText))
            {
                if (!TryParseDecimal(elevationText, out var parsedElevation))
                {
                    return ExternalWorkoutParseResult.Failure($"Invalid elevation in GPX point #{i + 1}.");
                }

                elevationMeters = parsedElevation;
            }

            var timeText = GetChildValue(trackPoint, "time");
            DateTimeOffset? recordedAt = null;

            if (!string.IsNullOrWhiteSpace(timeText))
            {
                if (!DateTimeOffset.TryParse(
                        timeText,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedTime))
                {
                    return ExternalWorkoutParseResult.Failure($"Invalid time in GPX point #{i + 1}.");
                }

                recordedAt = parsedTime;
            }

            points.Add(new ExternalWorkoutPoint
            {
                Order = i + 1,
                Latitude = latitude,
                Longitude = longitude,
                ElevationMeters = elevationMeters,
                RecordedAt = recordedAt
            });
        }

        var startedAt = points.FirstOrDefault(x => x.RecordedAt.HasValue)?.RecordedAt;
        var endedAt = points.LastOrDefault(x => x.RecordedAt.HasValue)?.RecordedAt;

        var workoutData = new ExternalWorkoutData
        {
            Source = WorkoutSource.Gpx,
            ExternalId = null,
            Name = GetTrackName(document),
            OriginalFileName = Path.GetFileName(fileName),
            StartedAt = startedAt,
            EndedAt = endedAt,
            SourceDistanceKm = null,
            SourceDurationSeconds = null,
            Points = points
        };

        return ExternalWorkoutParseResult.Success(workoutData);
    }

    private static string? GetTrackName(XDocument document)
    {
        var trackElement = document
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "trk");

        return trackElement?
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName == "name")?
            .Value
            .Trim();
    }

    private static string? GetChildValue(XElement element, string childName)
    {
        return element
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName == childName)?
            .Value
            .Trim();
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        return decimal.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);
    }
}