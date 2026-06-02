using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartRunTracker.Application.Common;
using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.RunnerProfiles;
using SmartRunTracker.Application.RunningGoals;
using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Application.Workouts;
using SmartRunTracker.Infrastructure.ExternalWorkouts.Gpx;
using SmartRunTracker.Infrastructure.Persistence;
using SmartRunTracker.Infrastructure.Persistence.Repositories;
using SmartRunTracker.Infrastructure.Persistence.Services;

namespace SmartRunTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IDemoUserService, DemoUserService>();
        services.AddScoped<IRunnerProfileRepository, RunnerProfileRepository>();
        services.AddScoped<IRunningGoalRepository, RunningGoalRepository>();
        services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();
        services.AddScoped<IDemoScenarioService, DemoScenarioService>();
        services.AddScoped<IExternalWorkoutFileParser, GpxWorkoutFileParser>();

        return services;
    }
}