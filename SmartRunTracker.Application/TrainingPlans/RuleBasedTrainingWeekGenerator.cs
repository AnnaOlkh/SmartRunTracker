using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.TrainingPlans;

public sealed class RuleBasedTrainingWeekGenerator : ITrainingWeekGenerator
{
    private const int SecondsInMinute = 60;
    private const int FiveMinutesInSeconds = 300;
    private const int MinimumSessionDurationSeconds = 900;
    private const int MaximumGeneratedWorkoutsPerWeek = 6;

    public GeneratedTrainingWeek Generate(TrainingWeekGenerationInput input)
    {
        ArgumentNullException.ThrowIfNull(input.ActiveGoal);
        ArgumentNullException.ThrowIfNull(input.RunnerProfile);
        ArgumentNullException.ThrowIfNull(input.RecentWorkouts);
        ArgumentNullException.ThrowIfNull(input.RecentPlannedSessions);

        var targetWorkoutCount = ResolveTargetWorkoutCount(input.RunnerProfile);

        var lastWeek = CreateWeeklySummary(
            input.RecentWorkouts,
            input.RecentPlannedSessions,
            input.WeekStartDate.AddDays(-7),
            input.WeekStartDate);

        var previousWeek = CreateWeeklySummary(
            input.RecentWorkouts,
            input.RecentPlannedSessions,
            input.WeekStartDate.AddDays(-14),
            input.WeekStartDate.AddDays(-7));

        var adherenceSignal = DetermineAdherenceSignal(
            targetWorkoutCount,
            lastWeek,
            previousWeek);

        var adjustmentMode = DetermineAdjustmentMode(
            lastWeek,
            previousWeek,
            adherenceSignal);

        var baseWeekDurationSeconds = DetermineBaseWeekDurationSeconds(
            targetWorkoutCount,
            lastWeek,
            previousWeek,
            adherenceSignal);

        var targetWeekDurationSeconds = ApplyAdjustment(
            baseWeekDurationSeconds,
            targetWorkoutCount,
            adjustmentMode,
            adherenceSignal);

        var sessions = GenerateSessions(
            targetWorkoutCount,
            targetWeekDurationSeconds,
            input.ActiveGoal.TargetPaceSecondsPerKm,
            adjustmentMode,
            adherenceSignal);

        var explanation = BuildExplanation(
            input.RunnerProfile,
            targetWorkoutCount,
            adjustmentMode,
            adherenceSignal,
            lastWeek,
            previousWeek,
            targetWeekDurationSeconds);

        return new GeneratedTrainingWeek(
            adjustmentMode,
            targetWorkoutCount,
            targetWeekDurationSeconds,
            explanation,
            sessions);
    }

    private static int ResolveTargetWorkoutCount(RunnerProfile profile)
    {
        var requestedWorkoutCount = Math.Clamp(
            profile.PreferredWorkoutsPerWeek,
            1,
            MaximumGeneratedWorkoutsPerWeek);

        if (profile.TrainingDayPreferenceMode != TrainingDayPreferenceMode.SelectedDays)
        {
            return requestedWorkoutCount;
        }

        var availableDayCount = profile.AvailableDays
            .Select(day => day.Day)
            .Distinct()
            .Count();

        if (availableDayCount <= 0)
        {
            return requestedWorkoutCount;
        }

        return Math.Min(requestedWorkoutCount, availableDayCount);
    }

    private static WeeklyTrainingSummary CreateWeeklySummary(
        IReadOnlyList<Workout> workouts,
        IReadOnlyList<PlannedSession> plannedSessions,
        DateOnly startDate,
        DateOnly endDate)
    {
        var start = ToUtcDateTimeOffset(startDate);
        var end = ToUtcDateTimeOffset(endDate);

        var weekWorkouts = workouts
            .Where(workout => workout.StartedAt >= start && workout.StartedAt < end)
            .ToList();

        var weekPlannedSessions = plannedSessions
            .Where(session => session.ScheduledFor is not null)
            .Where(session => session.ScheduledFor >= start && session.ScheduledFor < end)
            .ToList();

        var totalDurationSeconds = weekWorkouts.Sum(workout => workout.DurationSeconds);

        var averageRpe = weekWorkouts.Count == 0
            ? 0
            : weekWorkouts.Average(workout => workout.Rpe);

        var qualityWorkoutCount = weekWorkouts
            .Count(workout => workout.Type == WorkoutType.Quality);

        var skippedSessionCount = weekPlannedSessions
            .Count(session => session.Status == PlannedSessionStatus.Skipped);

        var completedPlannedSessionCount = weekPlannedSessions
            .Count(session => session.Status == PlannedSessionStatus.Completed);

        var plannedSessionCount = weekPlannedSessions.Count;

        var completionRate = plannedSessionCount == 0
            ? 1.0
            : completedPlannedSessionCount / (double)plannedSessionCount;

        return new WeeklyTrainingSummary(
            TotalDurationSeconds: totalDurationSeconds,
            AverageRpe: averageRpe,
            QualityWorkoutCount: qualityWorkoutCount,
            WorkoutCount: weekWorkouts.Count,
            PlannedSessionCount: plannedSessionCount,
            SkippedSessionCount: skippedSessionCount,
            CompletionRate: completionRate);
    }

    private static AdherenceSignal DetermineAdherenceSignal(
        int targetWorkoutCount,
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek)
    {
        var hasPlannedSessionData = lastWeek.PlannedSessionCount > 0;

        var hasManySkippedSessions = lastWeek.SkippedSessionCount >= 2;

        var hasLowCompletionRate = hasPlannedSessionData
            && lastWeek.PlannedSessionCount >= 2
            && lastWeek.CompletionRate < 0.5;

        var hadOnlyOneWorkoutAfterNormalWeek = lastWeek.WorkoutCount <= 1
            && previousWeek.WorkoutCount >= 2;

        var hadOnlyOneWorkoutComparedToTarget = targetWorkoutCount >= 3
            && lastWeek.WorkoutCount <= 1
            && lastWeek.TotalDurationSeconds > 0;

        if (hasManySkippedSessions || hasLowCompletionRate)
        {
            return AdherenceSignal.Cancellations;
        }

        if (hadOnlyOneWorkoutAfterNormalWeek || hadOnlyOneWorkoutComparedToTarget)
        {
            return AdherenceSignal.LowRecentConsistency;
        }

        return AdherenceSignal.Normal;
    }

    private static AdjustmentMode DetermineAdjustmentMode(
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek,
        AdherenceSignal adherenceSignal)
    {
        if (lastWeek.WorkoutCount == 0 && previousWeek.WorkoutCount == 0)
        {
            return AdjustmentMode.Maintain;
        }

        var durationIncreaseRatio = CalculateDurationIncreaseRatio(
            lastWeek,
            previousWeek);

        var hasHighAverageRpe = lastWeek.AverageRpe >= 8.0;
        var hasSharpVolumeIncrease = durationIncreaseRatio > 0.35;
        var hasTooManyQualityWorkouts = lastWeek.QualityWorkoutCount >= 2;

        if (hasHighAverageRpe || hasSharpVolumeIncrease || hasTooManyQualityWorkouts)
        {
            return AdjustmentMode.Deload;
        }

        if (adherenceSignal != AdherenceSignal.Normal)
        {
            return AdjustmentMode.Maintain;
        }

        var hasPreviousTrainingData = previousWeek.WorkoutCount > 0;
        var hasControlledRpe = lastWeek.AverageRpe > 0 && lastWeek.AverageRpe <= 6.5;
        var hasStableVolume = durationIncreaseRatio is >= -0.10 and <= 0.20;
        var hasControlledIntensity = lastWeek.QualityWorkoutCount <= 1;

        if (hasPreviousTrainingData
            && hasControlledRpe
            && hasStableVolume
            && hasControlledIntensity)
        {
            return AdjustmentMode.Progress;
        }

        return AdjustmentMode.Maintain;
    }

    private static double CalculateDurationIncreaseRatio(
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek)
    {
        if (previousWeek.TotalDurationSeconds <= 0)
        {
            return 0;
        }

        return (lastWeek.TotalDurationSeconds - previousWeek.TotalDurationSeconds)
            / (double)previousWeek.TotalDurationSeconds;
    }

    private static int DetermineBaseWeekDurationSeconds(
        int targetWorkoutCount,
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek,
        AdherenceSignal adherenceSignal)
    {
        var fallbackDurationSeconds = GetFallbackWeekDurationSeconds(targetWorkoutCount);

        if (lastWeek.TotalDurationSeconds <= 0 && previousWeek.TotalDurationSeconds <= 0)
        {
            return fallbackDurationSeconds;
        }

        if (adherenceSignal == AdherenceSignal.Cancellations
            || adherenceSignal == AdherenceSignal.LowRecentConsistency)
        {
            return DetermineConservativeReturnDuration(
                targetWorkoutCount,
                lastWeek,
                previousWeek,
                fallbackDurationSeconds);
        }

        var minimumReasonableWeekDuration = targetWorkoutCount * MinimumSessionDurationSeconds;

        if (lastWeek.TotalDurationSeconds < minimumReasonableWeekDuration)
        {
            return Math.Max(lastWeek.TotalDurationSeconds, fallbackDurationSeconds);
        }

        return lastWeek.TotalDurationSeconds;
    }

    private static int DetermineConservativeReturnDuration(
        int targetWorkoutCount,
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek,
        int fallbackDurationSeconds)
    {
        var minimumWeekDuration = targetWorkoutCount * MinimumSessionDurationSeconds;

        if (previousWeek.TotalDurationSeconds > 0)
        {
            var conservativePreviousWeekDuration = (int)Math.Round(
                previousWeek.TotalDurationSeconds * 0.75,
                MidpointRounding.AwayFromZero);

            return Math.Max(
                minimumWeekDuration,
                RoundToNearestFiveMinutes(conservativePreviousWeekDuration));
        }

        if (lastWeek.TotalDurationSeconds > 0)
        {
            var rebuiltDuration = Math.Max(
                lastWeek.TotalDurationSeconds * 2,
                (int)Math.Round(fallbackDurationSeconds * 0.70, MidpointRounding.AwayFromZero));

            return Math.Max(
                minimumWeekDuration,
                RoundToNearestFiveMinutes(rebuiltDuration));
        }

        return Math.Max(
            minimumWeekDuration,
            RoundToNearestFiveMinutes((int)Math.Round(
                fallbackDurationSeconds * 0.70,
                MidpointRounding.AwayFromZero)));
    }

    private static int GetFallbackWeekDurationSeconds(int targetWorkoutCount)
    {
        return targetWorkoutCount switch
        {
            1 => 2400,
            2 => 4800,
            3 => 6600,
            4 => 9000,
            5 => 10800,
            _ => 12600
        };
    }

    private static int ApplyAdjustment(
        int baseWeekDurationSeconds,
        int targetWorkoutCount,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        var factor = adjustmentMode switch
        {
            AdjustmentMode.Deload => 0.80,
            AdjustmentMode.Progress => 1.08,
            _ => 1.00
        };

        if (adherenceSignal != AdherenceSignal.Normal && adjustmentMode == AdjustmentMode.Maintain)
        {
            factor = 1.00;
        }

        var adjustedDuration = (int)Math.Round(
            baseWeekDurationSeconds * factor,
            MidpointRounding.AwayFromZero);

        var roundedDuration = RoundToNearestFiveMinutes(adjustedDuration);
        var minimumWeekDuration = targetWorkoutCount * MinimumSessionDurationSeconds;

        return Math.Max(roundedDuration, minimumWeekDuration);
    }

    private static IReadOnlyList<GeneratedPlannedSession> GenerateSessions(
        int targetWorkoutCount,
        int targetWeekDurationSeconds,
        int targetPaceSecondsPerKm,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        var workoutTypes = BuildWorkoutTypePattern(
            targetWorkoutCount,
            adjustmentMode,
            adherenceSignal);

        var durations = DistributeDuration(
            targetWeekDurationSeconds,
            workoutTypes);

        return workoutTypes
            .Select((type, index) =>
            {
                var durationSeconds = durations[index];

                var paceSecondsPerKm = CalculateRecommendedPace(
                    type,
                    targetPaceSecondsPerKm,
                    adjustmentMode,
                    adherenceSignal);

                var distanceKm = CalculateTargetDistance(
                    durationSeconds,
                    paceSecondsPerKm);

                return new GeneratedPlannedSession(
                    Type: type,
                    Intensity: CalculateIntensity(type, adjustmentMode, adherenceSignal),
                    TargetDurationSeconds: durationSeconds,
                    TargetDistanceKm: distanceKm,
                    TargetPaceSecondsPerKm: paceSecondsPerKm,
                    Notes: BuildNotes(type, adjustmentMode, adherenceSignal),
                    Reason: BuildReason(type, adjustmentMode, adherenceSignal),
                    SortOrder: index + 1);
            })
            .ToList();
    }

    private static IReadOnlyList<WorkoutType> BuildWorkoutTypePattern(
        int targetWorkoutCount,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        if (adjustmentMode == AdjustmentMode.Deload
            || adherenceSignal != AdherenceSignal.Normal)
        {
            return targetWorkoutCount switch
            {
                1 => new List<WorkoutType>
                {
                    WorkoutType.Recovery
                },

                2 => new List<WorkoutType>
                {
                    WorkoutType.Recovery,
                    WorkoutType.Easy
                },

                3 => new List<WorkoutType>
                {
                    WorkoutType.Recovery,
                    WorkoutType.Easy,
                    WorkoutType.Long
                },

                4 => new List<WorkoutType>
                {
                    WorkoutType.Recovery,
                    WorkoutType.Easy,
                    WorkoutType.Easy,
                    WorkoutType.Long
                },

                5 => new List<WorkoutType>
                {
                    WorkoutType.Recovery,
                    WorkoutType.Easy,
                    WorkoutType.Easy,
                    WorkoutType.Recovery,
                    WorkoutType.Long
                },

                _ => new List<WorkoutType>
                {
                    WorkoutType.Recovery,
                    WorkoutType.Easy,
                    WorkoutType.Easy,
                    WorkoutType.Recovery,
                    WorkoutType.Easy,
                    WorkoutType.Long
                }
            };
        }

        return targetWorkoutCount switch
        {
            1 => new List<WorkoutType>
            {
                WorkoutType.Easy
            },

            2 => new List<WorkoutType>
            {
                WorkoutType.Easy,
                WorkoutType.Long
            },

            3 => new List<WorkoutType>
            {
                WorkoutType.Easy,
                WorkoutType.Quality,
                WorkoutType.Long
            },

            4 => new List<WorkoutType>
            {
                WorkoutType.Easy,
                WorkoutType.Quality,
                WorkoutType.Easy,
                WorkoutType.Long
            },

            5 => new List<WorkoutType>
            {
                WorkoutType.Recovery,
                WorkoutType.Easy,
                WorkoutType.Quality,
                WorkoutType.Easy,
                WorkoutType.Long
            },

            _ => new List<WorkoutType>
            {
                WorkoutType.Recovery,
                WorkoutType.Easy,
                WorkoutType.Quality,
                WorkoutType.Easy,
                WorkoutType.Recovery,
                WorkoutType.Long
            }
        };
    }

    private static IReadOnlyList<int> DistributeDuration(
        int targetWeekDurationSeconds,
        IReadOnlyList<WorkoutType> workoutTypes)
    {
        var weights = workoutTypes
            .Select(GetDurationWeight)
            .ToList();

        var totalWeight = weights.Sum();

        var durations = weights
            .Select(weight =>
            {
                var rawDuration = targetWeekDurationSeconds * (weight / totalWeight);

                var roundedDuration = RoundToNearestFiveMinutes(
                    (int)Math.Round(rawDuration, MidpointRounding.AwayFromZero));

                return Math.Max(roundedDuration, MinimumSessionDurationSeconds);
            })
            .ToList();

        var difference = targetWeekDurationSeconds - durations.Sum();

        durations[^1] = Math.Max(
            MinimumSessionDurationSeconds,
            durations[^1] + difference);

        return durations;
    }

    private static decimal GetDurationWeight(WorkoutType type)
    {
        return type switch
        {
            WorkoutType.Recovery => 0.75m,
            WorkoutType.Easy => 1.00m,
            WorkoutType.Quality => 1.15m,
            WorkoutType.Long => 1.80m,
            _ => 1.00m
        };
    }

    private static WorkoutIntensity CalculateIntensity(
        WorkoutType type,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        if (adjustmentMode == AdjustmentMode.Deload
            || adherenceSignal != AdherenceSignal.Normal)
        {
            return type == WorkoutType.Long
                ? WorkoutIntensity.Moderate
                : WorkoutIntensity.Low;
        }

        return type switch
        {
            WorkoutType.Recovery => WorkoutIntensity.Low,
            WorkoutType.Easy => WorkoutIntensity.Low,
            WorkoutType.Long => WorkoutIntensity.Moderate,
            WorkoutType.Quality when adjustmentMode == AdjustmentMode.Progress => WorkoutIntensity.High,
            WorkoutType.Quality => WorkoutIntensity.Moderate,
            _ => WorkoutIntensity.Moderate
        };
    }

    private static int CalculateRecommendedPace(
        WorkoutType type,
        int targetPaceSecondsPerKm,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        var paceOffset = type switch
        {
            WorkoutType.Recovery => 70,
            WorkoutType.Easy => 40,
            WorkoutType.Long => 50,
            WorkoutType.Quality when adjustmentMode == AdjustmentMode.Progress
                && adherenceSignal == AdherenceSignal.Normal => 15,
            WorkoutType.Quality => 25,
            _ => 40
        };

        return targetPaceSecondsPerKm + paceOffset;
    }

    private static decimal CalculateTargetDistance(
        int durationSeconds,
        int paceSecondsPerKm)
    {
        if (paceSecondsPerKm <= 0)
        {
            return 0;
        }

        return Math.Round(
            durationSeconds / (decimal)paceSecondsPerKm,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static string BuildNotes(
        WorkoutType type,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        if (adherenceSignal != AdherenceSignal.Normal)
        {
            return type switch
            {
                WorkoutType.Recovery => "Very easy recovery run after an inconsistent week.",
                WorkoutType.Easy => "Easy controlled run to rebuild training consistency.",
                WorkoutType.Long => "Reduced long run focused on comfortable endurance.",
                _ => "Controlled session after an inconsistent training week."
            };
        }

        if (adjustmentMode == AdjustmentMode.Deload)
        {
            return type switch
            {
                WorkoutType.Recovery => "Very easy recovery run during a deload week.",
                WorkoutType.Easy => "Easy run with reduced training stress.",
                WorkoutType.Long => "Reduced long run. Keep the effort comfortable.",
                _ => "Reduced session during a deload week."
            };
        }

        return type switch
        {
            WorkoutType.Recovery => "Very easy recovery run. Keep the effort comfortable.",
            WorkoutType.Easy => "Easy run at a controlled conversational pace.",
            WorkoutType.Long => "Long endurance run. Keep the pace comfortable and stable.",
            WorkoutType.Quality when adjustmentMode == AdjustmentMode.Progress =>
                "Quality workout with controlled faster segments. Avoid maximal effort.",
            WorkoutType.Quality =>
                "Moderate quality workout. Keep intensity controlled.",
            _ => "Planned running session."
        };
    }

    private static string BuildReason(
        WorkoutType type,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal)
    {
        if (adherenceSignal == AdherenceSignal.Cancellations)
        {
            return type switch
            {
                WorkoutType.Recovery => "Added because several planned sessions were skipped last week.",
                WorkoutType.Easy => "Added to restore consistency without sharply increasing load.",
                WorkoutType.Long => "Kept as a reduced endurance session after missed training.",
                _ => "Added as part of a conservative return after cancellations."
            };
        }

        if (adherenceSignal == AdherenceSignal.LowRecentConsistency)
        {
            return type switch
            {
                WorkoutType.Recovery => "Added because last week had very low completed training volume.",
                WorkoutType.Easy => "Added to rebuild weekly rhythm after low consistency.",
                WorkoutType.Long => "Kept in reduced form to maintain endurance structure.",
                _ => "Added as part of a conservative return to training."
            };
        }

        if (adjustmentMode == AdjustmentMode.Deload)
        {
            return type switch
            {
                WorkoutType.Recovery => "Added to reduce training stress during a deload week.",
                WorkoutType.Easy => "Added to maintain running frequency without increasing load.",
                WorkoutType.Long => "Kept as a reduced endurance session for weekly structure.",
                _ => "Added as part of the deload structure."
            };
        }

        if (adjustmentMode == AdjustmentMode.Progress)
        {
            return type switch
            {
                WorkoutType.Quality => "Added because recent training looks stable enough for controlled progression.",
                WorkoutType.Long => "Added to gradually improve endurance capacity.",
                WorkoutType.Easy => "Added to support aerobic volume without excessive intensity.",
                WorkoutType.Recovery => "Added to balance the increased training load.",
                _ => "Added as part of the progression structure."
            };
        }

        return type switch
        {
            WorkoutType.Quality => "Added to keep one controlled quality stimulus in the week.",
            WorkoutType.Long => "Added to maintain endurance development.",
            WorkoutType.Easy => "Added to maintain aerobic consistency.",
            WorkoutType.Recovery => "Added to support recovery and training balance.",
            _ => "Added as part of the weekly training structure."
        };
    }

    private static string BuildExplanation(
        RunnerProfile profile,
        int targetWorkoutCount,
        AdjustmentMode adjustmentMode,
        AdherenceSignal adherenceSignal,
        WeeklyTrainingSummary lastWeek,
        WeeklyTrainingSummary previousWeek,
        int targetWeekDurationSeconds)
    {
        var targetMinutes = targetWeekDurationSeconds / SecondsInMinute;

        var availabilityText = BuildAvailabilityText(profile, targetWorkoutCount);

        var adherenceText = adherenceSignal switch
        {
            AdherenceSignal.Cancellations =>
                "Several planned sessions were skipped last week, so the next week is generated as a conservative return to training.",

            AdherenceSignal.LowRecentConsistency =>
                "Last week had very low completed training volume, so the next week avoids progression and rebuilds consistency.",

            _ =>
                "Recent training adherence does not require a special correction."
        };

        var modeText = adjustmentMode switch
        {
            AdjustmentMode.Deload =>
                "Deload mode was selected because recent training shows elevated load indicators.",

            AdjustmentMode.Progress =>
                "Progress mode was selected because recent training volume and RPE look stable.",

            _ =>
                "Maintain mode was selected because there is no strong signal for either progression or deload."
        };

        return $"{modeText} {adherenceText} {availabilityText} " +
               $"Last week: {lastWeek.TotalDurationSeconds / SecondsInMinute} min, " +
               $"completed workouts: {lastWeek.WorkoutCount}, " +
               $"skipped sessions: {lastWeek.SkippedSessionCount}, " +
               $"average RPE: {lastWeek.AverageRpe:F1}. " +
               $"Previous week: {previousWeek.TotalDurationSeconds / SecondsInMinute} min. " +
               $"Generated target: {targetWorkoutCount} sessions, {targetMinutes} min total.";
    }

    private static string BuildAvailabilityText(
        RunnerProfile profile,
        int targetWorkoutCount)
    {
        if (profile.TrainingDayPreferenceMode != TrainingDayPreferenceMode.SelectedDays)
        {
            return "No fixed available days were selected, so the plan uses the preferred weekly workout count.";
        }

        var availableDayCount = profile.AvailableDays
            .Select(day => day.Day)
            .Distinct()
            .Count();

        if (availableDayCount == 0)
        {
            return "No available days were provided, so the plan uses the preferred weekly workout count.";
        }

        if (availableDayCount > targetWorkoutCount)
        {
            return $"The user selected {availableDayCount} available days, but only {targetWorkoutCount} sessions are generated because available days are possible scheduling slots, not required workout count.";
        }

        return $"The plan uses {targetWorkoutCount} sessions based on the selected available training days.";
    }

    private static int RoundToNearestFiveMinutes(int seconds)
    {
        return (int)Math.Round(
            seconds / (double)FiveMinutesInSeconds,
            MidpointRounding.AwayFromZero) * FiveMinutesInSeconds;
    }

    private static DateTimeOffset ToUtcDateTimeOffset(DateOnly date)
    {
        return new DateTimeOffset(
            date.ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero);
    }

    private sealed record WeeklyTrainingSummary(
        int TotalDurationSeconds,
        double AverageRpe,
        int QualityWorkoutCount,
        int WorkoutCount,
        int PlannedSessionCount,
        int SkippedSessionCount,
        double CompletionRate);

    private enum AdherenceSignal
    {
        Normal = 1,
        Cancellations = 2,
        LowRecentConsistency = 3
    }
}