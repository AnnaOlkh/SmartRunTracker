using Microsoft.EntityFrameworkCore;
using SmartRunTracker.Application.TrainingPlans;
using SmartRunTracker.Domain.Entities;
using SmartRunTracker.Domain.Enums;

namespace SmartRunTracker.Infrastructure.Persistence.Repositories;

public sealed class TrainingPlanRepository : ITrainingPlanRepository
{
    private readonly AppDbContext _dbContext;

    public TrainingPlanRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TrainingWeek?> GetByWeekStartDateAsync(
        int userId,
        DateOnly weekStartDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TrainingWeeks
            .AsNoTracking()
            .Include(week => week.PlannedSessions)
            .FirstOrDefaultAsync(
                week => week.UserId == userId && week.WeekStartDate == weekStartDate,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingWeek>> GetWeeksAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TrainingWeeks
            .AsNoTracking()
            .Include(week => week.PlannedSessions)
            .Where(week => week.UserId == userId)
            .OrderByDescending(week => week.WeekStartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingWeek?> GetWeekByIdAsync(
        int userId,
        int trainingWeekId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TrainingWeeks
            .AsNoTracking()
            .Include(week => week.PlannedSessions)
            .FirstOrDefaultAsync(
                week => week.Id == trainingWeekId && week.UserId == userId,
                cancellationToken);
    }

    public async Task<TrainingWeek> AddWeekAsync(
        TrainingWeek trainingWeek,
        CancellationToken cancellationToken = default)
    {
        _dbContext.TrainingWeeks.Add(trainingWeek);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return trainingWeek;
    }

    public async Task<PlannedSession?> GetPlannedSessionByIdAsync(
        int userId,
        int plannedSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlannedSessions
            .Include(session => session.TrainingWeek)
            .FirstOrDefaultAsync(
                session => session.Id == plannedSessionId
                    && session.TrainingWeek.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PlannedSession>> GetRecentPlannedSessionsAsync(
        int userId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlannedSessions
            .AsNoTracking()
            .Include(session => session.TrainingWeek)
            .Where(session => session.TrainingWeek.UserId == userId)
            .Where(session => session.ScheduledFor != null)
            .Where(session => session.ScheduledFor >= from && session.ScheduledFor < to)
            .OrderByDescending(session => session.ScheduledFor)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePlannedSessionAsync(
        PlannedSession plannedSession,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PlannedSessions.Update(plannedSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkOverdueScheduledSessionsAsSkippedAsync(
    int userId,
    DateTimeOffset now,
    CancellationToken cancellationToken = default)
    {
        var overdueSessions = await _dbContext.PlannedSessions
            .Include(session => session.TrainingWeek)
            .Where(session => session.TrainingWeek.UserId == userId)
            .Where(session => session.Status == PlannedSessionStatus.Scheduled)
            .Where(session => session.ScheduledFor != null)
            .Where(session => session.ScheduledFor < now)
            .ToListAsync(cancellationToken);

        if (overdueSessions.Count == 0)
        {
            return;
        }

        foreach (var session in overdueSessions)
        {
            session.Status = PlannedSessionStatus.Skipped;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}