using Microsoft.Extensions.DependencyInjection;
using SmartRunTracker.Application.RunnerProfiles;
using SmartRunTracker.Application.RunningGoals;
using SmartRunTracker.Application.Workouts;

namespace SmartRunTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IRunnerProfileService, RunnerProfileService>();
        services.AddScoped<IRunningGoalService, RunningGoalService>();
        return services;
    }
}