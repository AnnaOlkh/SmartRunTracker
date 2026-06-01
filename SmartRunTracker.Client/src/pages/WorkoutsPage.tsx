import { useMemo, useState } from "react";
import type {
  PlannedSessionDto,
  RunningGoalDto,
  TrainingWeekDto,
  WorkoutDto,
  WorkoutType,
} from "../api/types";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { useActiveRunningGoal } from "../hooks/useRunningGoals";
import { useTrainingWeeks } from "../hooks/useTrainingWeeks";
import {
  useCreateWorkout,
  useDeleteWorkout,
  useWorkouts,
} from "../hooks/useWorkouts";
import {
  formatDateTime,
  formatDuration,
  formatPace,
  toDateTimeLocalValue,
  toLocalDateKey,
} from "../utils/format";

const workoutTypes: WorkoutType[] = ["Easy", "Quality", "Long", "Recovery"];

export function WorkoutsPage() {
  const workouts = useWorkouts();
  const trainingWeeks = useTrainingWeeks();
  const activeGoal = useActiveRunningGoal();

  const createWorkout = useCreateWorkout();
  const deleteWorkout = useDeleteWorkout();

  const [startedAt, setStartedAt] = useState(toDateTimeLocalValue());
  const [distanceKm, setDistanceKm] = useState("5");
  const [hours, setHours] = useState("0");
  const [minutes, setMinutes] = useState("30");
  const [seconds, setSeconds] = useState("0");
  const [rpe, setRpe] = useState("5");
  const [type, setType] = useState<WorkoutType>("Easy");
  const [plannedSessionId, setPlannedSessionId] = useState("");
  const [notes, setNotes] = useState("");
  const [workoutIdPendingDelete, setWorkoutIdPendingDelete] = useState<
    number | null
  >(null);

  const availablePlannedSessions = useMemo(() => {
    return getPlannedSessionsForStartedAtWeek(
      trainingWeeks.data ?? [],
      startedAt,
    );
  }, [trainingWeeks.data, startedAt]);

  const formValidationError = getWorkoutFormValidationError({
    startedAt,
    distanceKm,
    hours,
    minutes,
    seconds,
    rpe,
  });

  const goalAchievementMessage =
    createWorkout.data && activeGoal.data
      ? getGoalAchievementMessage(createWorkout.data, activeGoal.data)
      : null;

  function handleStartedAtChange(value: string) {
    setStartedAt(value);
    setPlannedSessionId("");
  }

  function handlePlannedSessionChange(value: string) {
    setPlannedSessionId(value);

    if (value === "") {
      return;
    }

    const selectedSession = availablePlannedSessions.find(
      (session) => session.id === Number(value),
    );

    if (!selectedSession) {
      return;
    }

    setType(selectedSession.type);

    if (selectedSession.targetDistanceKm !== null) {
      setDistanceKm(String(selectedSession.targetDistanceKm));
    }

    const duration = selectedSession.targetDurationSeconds;

    setHours(String(Math.floor(duration / 3600)));
    setMinutes(String(Math.floor((duration % 3600) / 60)));
    setSeconds(String(duration % 60));

    setNotes(
      selectedSession.notes
        ? `Completed planned session: ${selectedSession.notes}`
        : "Completed planned session.",
    );
  }

  function handleCreate() {
    if (formValidationError !== null) {
      return;
    }

    const durationSeconds =
      Number(hours) * 3600 + Number(minutes) * 60 + Number(seconds);

    createWorkout.mutate({
      startedAt: new Date(startedAt).toISOString(),
      distanceKm: Number(distanceKm),
      durationSeconds,
      rpe: Number(rpe),
      type,
      notes: notes.trim() === "" ? null : notes.trim(),
      plannedSessionId:
        plannedSessionId.trim() === "" ? null : Number(plannedSessionId),
    });
  }

  function confirmDeleteWorkout() {
    if (workoutIdPendingDelete === null) {
      return;
    }

    deleteWorkout.mutate(workoutIdPendingDelete, {
      onSuccess: () => {
        setWorkoutIdPendingDelete(null);
      },
    });
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Workouts</h2>
          <p>Add completed runs and link them to planned sessions.</p>
        </div>
      </div>

      <section className="card">
        <h3>Add workout</h3>

        <div className="form-grid">
          <label className="field">
            <span>Started at</span>
            <input
              type="datetime-local"
              max={toDateTimeLocalValue()}
              value={startedAt}
              onChange={(event) => handleStartedAtChange(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Link to planned session, optional</span>
            <select
              value={plannedSessionId}
              onChange={(event) => handlePlannedSessionChange(event.target.value)}
            >
              <option value="">Do not link</option>

              {availablePlannedSessions.map((session) => (
                <option key={session.id} value={session.id}>
                  {formatPlannedSessionOption(session)}
                </option>
              ))}
            </select>
          </label>

          <label className="field">
            <span>Distance, km</span>
            <input
              type="number"
              min="0"
              step="0.01"
              value={distanceKm}
              onChange={(event) => setDistanceKm(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Hours</span>
            <input
              type="number"
              min="0"
              value={hours}
              onChange={(event) => setHours(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Minutes</span>
            <input
              type="number"
              min="0"
              max="59"
              value={minutes}
              onChange={(event) => setMinutes(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Seconds</span>
            <input
              type="number"
              min="0"
              max="59"
              value={seconds}
              onChange={(event) => setSeconds(event.target.value)}
            />
          </label>

          <label className="field">
            <span>RPE</span>
            <input
              type="number"
              min="1"
              max="10"
              value={rpe}
              onChange={(event) => setRpe(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Type</span>
            <select
              value={type}
              onChange={(event) => setType(event.target.value as WorkoutType)}
            >
              {workoutTypes.map((workoutType) => (
                <option key={workoutType} value={workoutType}>
                  {workoutType}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="field" style={{ marginTop: 14 }}>
          <span>Notes</span>
          <textarea
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
          />
        </label>

        {formValidationError && <p className="error">{formValidationError}</p>}

        <div className="actions">
          <button
            type="button"
            onClick={handleCreate}
            disabled={createWorkout.isPending || formValidationError !== null}
          >
            {createWorkout.isPending ? "Saving..." : "Add workout"}
          </button>
        </div>

        {createWorkout.error && (
          <p className="error">{createWorkout.error.message}</p>
        )}

        {createWorkout.data && <p className="success">Workout saved.</p>}

        {goalAchievementMessage && (
          <div className="goal-notification">
            <strong>Goal matched.</strong>
            <p>{goalAchievementMessage}</p>
          </div>
        )}
      </section>

      <section className="card">
        <h3>Workout history</h3>

        {workouts.isLoading && <p className="muted">Loading workouts...</p>}

        {workouts.error && <p className="error">{workouts.error.message}</p>}

        {workouts.data?.length === 0 && (
          <p className="muted">No workouts yet.</p>
        )}

        <div className="scroll-panel">
          <div className="list">
            {workouts.data?.map((workout) => (
              <article key={workout.id} className="list-item">
                <div className="list-item-header">
                  <div>
                    <h4>{workout.type}</h4>
                    <p className="muted">{formatDateTime(workout.startedAt)}</p>
                  </div>

                  <button
                    type="button"
                    className="danger-button"
                    onClick={() => setWorkoutIdPendingDelete(workout.id)}
                    disabled={deleteWorkout.isPending}
                  >
                    Delete
                  </button>
                </div>

                <p>
                  {workout.distanceKm} km ·{" "}
                  {formatDuration(workout.durationSeconds)} ·{" "}
                  {formatPace(workout.averagePaceSecondsPerKm)}
                </p>

                <p className="muted">
                  RPE: {workout.rpe} · Load: {workout.sessionLoad}
                  {workout.plannedSessionId
                    ? " · Linked planned session"
                    : ""}
                </p>

                {workout.notes && <p>{workout.notes}</p>}
              </article>
            ))}
          </div>
        </div>
      </section>

      {workoutIdPendingDelete !== null && (
        <ConfirmDialog
          title="Delete workout"
          message="This workout will be permanently removed from the workout history. Continue?"
          onConfirm={confirmDeleteWorkout}
          onCancel={() => setWorkoutIdPendingDelete(null)}
        />
      )}
    </div>
  );
}

interface WorkoutFormValues {
  startedAt: string;
  distanceKm: string;
  hours: string;
  minutes: string;
  seconds: string;
  rpe: string;
}

function getWorkoutFormValidationError(values: WorkoutFormValues): string | null {
  if (!values.startedAt || Number.isNaN(new Date(values.startedAt).getTime())) {
    return "Started at date is required.";
  }

  if (new Date(values.startedAt) > new Date()) {
    return "Started at cannot be in the future.";
  }

  if (Number(values.distanceKm) <= 0) {
    return "Distance must be greater than zero.";
  }

  const hours = Number(values.hours);
  const minutes = Number(values.minutes);
  const seconds = Number(values.seconds);

  if (hours < 0 || minutes < 0 || seconds < 0) {
    return "Duration values cannot be negative.";
  }

  if (minutes > 59 || seconds > 59) {
    return "Minutes and seconds must be between 0 and 59.";
  }

  const durationSeconds = hours * 3600 + minutes * 60 + seconds;

  if (durationSeconds <= 0) {
    return "Duration must be greater than zero.";
  }

  const rpe = Number(values.rpe);

  if (rpe < 1 || rpe > 10) {
    return "RPE must be between 1 and 10.";
  }

  return null;
}

function getPlannedSessionsForStartedAtWeek(
  trainingWeeks: TrainingWeekDto[],
  startedAtLocalValue: string,
): PlannedSessionDto[] {
  const selectedDate = new Date(startedAtLocalValue);

  if (Number.isNaN(selectedDate.getTime())) {
    return [];
  }

  const weekStart = getMondayStart(selectedDate);
  const weekStartKey = toLocalDateKey(weekStart);

  const matchingWeek = trainingWeeks.find((week) => {
    return week.weekStartDate === weekStartKey;
  });

  if (!matchingWeek) {
    return [];
  }

  return matchingWeek.plannedSessions
  .filter((session) => {
    return (
      session.status === "Unscheduled" ||
      session.status === "Scheduled" ||
      session.status === "Skipped"
    );
  })
  .sort((a, b) => {
    const statusOrder = getPlannedSessionStatusOrder(a.status)
      - getPlannedSessionStatusOrder(b.status);

    if (statusOrder !== 0) {
      return statusOrder;
    }

    return a.sortOrder - b.sortOrder;
  });
}
function getPlannedSessionStatusOrder(status: PlannedSessionDto["status"]): number {
  switch (status) {
    case "Scheduled":
      return 1;

    case "Skipped":
      return 2;

    case "Unscheduled":
      return 3;

    case "Completed":
      return 4;

    default:
      return 5;
  }
}
function getMondayStart(date: Date): Date {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);

  const mondayBasedDayIndex = (result.getDay() + 6) % 7;
  result.setDate(result.getDate() - mondayBasedDayIndex);

  return result;
}

function formatPlannedSessionOption(session: PlannedSessionDto): string {
  const scheduledLabel = session.scheduledFor
    ? formatDateTime(session.scheduledFor)
    : "Unscheduled";

  return `${session.status} · ${scheduledLabel} · ${session.type} · ${formatDuration(
    session.targetDurationSeconds,
  )} · ${session.targetDistanceKm ?? "-"} km`;
}

function getGoalAchievementMessage(
  workout: WorkoutDto,
  goal: RunningGoalDto,
): string | null {
  const distanceReached = workout.distanceKm >= goal.targetDistanceKm;
  const paceReached =
    workout.averagePaceSecondsPerKm <= goal.targetPaceSecondsPerKm;

  if (distanceReached && paceReached) {
    return `This workout reaches the active goal: ${goal.targetDistanceKm} km at ${formatPace(
      goal.targetPaceSecondsPerKm,
    )} or faster.`;
  }

  if (distanceReached) {
    return `This workout reaches the target distance of ${goal.targetDistanceKm} km, but not the target pace.`;
  }

  return null;
}