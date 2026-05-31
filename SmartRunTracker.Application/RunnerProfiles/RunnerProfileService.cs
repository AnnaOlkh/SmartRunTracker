using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Application.RunnerProfiles;

public sealed class RunnerProfileService : IRunnerProfileService
{
    private readonly IRunnerProfileRepository _runnerProfileRepository;

    public RunnerProfileService(IRunnerProfileRepository runnerProfileRepository)
    {
        _runnerProfileRepository = runnerProfileRepository;
    }

    public async Task<RunnerProfileDto> GetOrCreateAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _runnerProfileRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (profile is not null)
        {
            return ToDto(profile);
        }

        var newProfile = new RunnerProfile
        {
            UserId = userId,
            PreferredWorkoutsPerWeek = 3,
            TrainingDayPreferenceMode = TrainingDayPreferenceMode.AnyDay,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdProfile = await _runnerProfileRepository.AddAsync(
            newProfile,
            cancellationToken);

        return ToDto(createdProfile);
    }

    public async Task<RunnerProfileDto> UpdateAsync(
        int userId,
        UpdateRunnerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var profile = await _runnerProfileRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (profile is null)
        {
            profile = new RunnerProfile
            {
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            ApplyChanges(profile, request);

            var createdProfile = await _runnerProfileRepository.AddAsync(
                profile,
                cancellationToken);

            return ToDto(createdProfile);
        }

        ApplyChanges(profile, request);

        await _runnerProfileRepository.UpdateAsync(
            profile,
            cancellationToken);

        return ToDto(profile);
    }

    private static void ApplyChanges(
        RunnerProfile profile,
        UpdateRunnerProfileRequest request)
    {
        profile.PreferredWorkoutsPerWeek = request.PreferredWorkoutsPerWeek;
        profile.TrainingDayPreferenceMode = request.TrainingDayPreferenceMode;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        profile.AvailableDays.Clear();

        if (request.TrainingDayPreferenceMode == TrainingDayPreferenceMode.SelectedDays)
        {
            foreach (var day in request.AvailableDays.Distinct())
            {
                profile.AvailableDays.Add(new RunnerAvailableDay
                {
                    Day = day
                });
            }
        }
    }

    private static void ValidateRequest(UpdateRunnerProfileRequest request)
    {
        if (request.PreferredWorkoutsPerWeek is < 1 or > 7)
        {
            throw new ArgumentException(
                "Preferred workouts per week must be between 1 and 7.");
        }

        if (request.TrainingDayPreferenceMode == TrainingDayPreferenceMode.SelectedDays
            && request.AvailableDays.Count == 0)
        {
            throw new ArgumentException(
                "At least one available day must be selected.");
        }

        var hasDuplicateDays = request.AvailableDays
            .GroupBy(day => day)
            .Any(group => group.Count() > 1);

        if (hasDuplicateDays)
        {
            throw new ArgumentException(
                "Available days must not contain duplicates.");
        }
    }

    private static RunnerProfileDto ToDto(RunnerProfile profile)
    {
        var availableDays = profile.AvailableDays
            .OrderBy(day => day.Day)
            .Select(day => day.Day)
            .ToList();

        return new RunnerProfileDto(
            profile.Id,
            profile.UserId,
            profile.PreferredWorkoutsPerWeek,
            profile.TrainingDayPreferenceMode,
            availableDays,
            profile.CreatedAt,
            profile.UpdatedAt
        );
    }
}