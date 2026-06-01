import type { TrainingWeekDto } from "../api/types";
import { useActiveRunningGoal } from "../hooks/useRunningGoals";
import { useRunnerProfile } from "../hooks/useRunnerProfile";
import { useTrainingWeeks } from "../hooks/useTrainingWeeks";
import { useWorkouts } from "../hooks/useWorkouts";
import { formatDuration, formatPace } from "../utils/format";

export function DashboardPage() {
  const workouts = useWorkouts();
  const profile = useRunnerProfile();
  const activeGoal = useActiveRunningGoal();
  const trainingWeeks = useTrainingWeeks();

  const currentWeek = findCurrentTrainingWeek(trainingWeeks.data ?? []);

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Dashboard</h2>
          <p>Overview of current goal, runner profile, workouts and plan.</p>
        </div>
      </div>

      <div className="stats-grid">
        <section className="card">
          <h3>Active goal</h3>

          {activeGoal.data ? (
            <>
              <p className="metric">{activeGoal.data.targetDistanceKm} km</p>
              <p className="muted">
                Target pace: {formatPace(activeGoal.data.targetPaceSecondsPerKm)}
              </p>
            </>
          ) : (
            <p className="muted">No active goal yet.</p>
          )}
        </section>

        <section className="card">
          <h3>Runner profile</h3>

          {profile.data ? (
            <>
              <p className="metric">
                {profile.data.preferredWorkoutsPerWeek} / week
              </p>
              <p className="muted">
                Mode: {profile.data.trainingDayPreferenceMode}
              </p>
            </>
          ) : (
            <p className="muted">Profile is not loaded.</p>
          )}
        </section>

        <section className="card">
          <h3>Workout history</h3>

          <p className="metric">{workouts.data?.length ?? 0}</p>
          <p className="muted">Saved workouts</p>
        </section>

        <section className="card">
          <h3>Current week plan</h3>

          {currentWeek ? (
            <>
              <p className="metric">{currentWeek.adjustmentMode}</p>
              <p className="muted">
                {currentWeek.targetWorkoutCount} sessions ·{" "}
                {formatDuration(currentWeek.targetWeekDurationSeconds)}
              </p>
            </>
          ) : (
            <p className="muted">No plan for the current week.</p>
          )}
        </section>
      </div>

      {currentWeek && (
        <section className="card">
          <h3>Current generated week</h3>
          <p className="muted">{currentWeek.explanation}</p>
        </section>
      )}
    </div>
  );
}

function findCurrentTrainingWeek(
  weeks: TrainingWeekDto[],
): TrainingWeekDto | null {
  const now = new Date();

  return (
    weeks.find((week) => {
      const start = new Date(week.weekStartDate);
      start.setHours(0, 0, 0, 0);

      const end = new Date(start);
      end.setDate(end.getDate() + 7);

      return now >= start && now < end;
    }) ?? null
  );
}