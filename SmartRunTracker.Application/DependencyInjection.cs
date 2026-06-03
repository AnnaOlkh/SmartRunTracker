using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartRunTracker.Application.Auth;
using SmartRunTracker.Application.ExternalWorkouts;
using SmartRunTracker.Application.ExternalWorkouts.Abstractions;
using SmartRunTracker.Application.ExternalWorkouts.Analysis;
using SmartRunTracker.Application.RunnerProfiles;
using SmartRunTracker.Application.RunningGoals;
using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Application.Workouts;
using SmartRunTracker.Application.Workouts.Analysis;
using SmartRunTracker.Domain.Entities;

namespace SmartRunTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IRunnerProfileService, RunnerProfileService>();
        services.AddScoped<IRunningGoalService, RunningGoalService>();

        services.AddScoped<ITrainingWeekGenerator, RuleBasedTrainingWeekGenerator>();
        services.AddScoped<ITrainingPlanService, TrainingPlanService>();
        services.AddScoped<IWorkoutRouteAnalyzer, WorkoutRouteAnalyzer>();
        services.AddScoped<IExternalWorkoutImportService, ExternalWorkoutImportService>();
        services.AddScoped<IPlannedVsActualAnalyzer, PlannedVsActualAnalyzer>();
        services.AddScoped<IWorkoutInsightBuilder, WorkoutInsightBuilder>();
        return services;
    }
}