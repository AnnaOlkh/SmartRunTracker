import { useEffect, useMemo, useState } from "react";
import type {
  PlannedSessionDto,
  RunningGoalDto,
  TrainingWeekDto,
  WorkoutDto,
  WorkoutType,
} from "../api/types";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { WorkoutDetailsPanel } from "../components/WorkoutDetailsPanel";
import { useActiveRunningGoal } from "../hooks/useRunningGoals";
import { useTrainingWeeks } from "../hooks/useTrainingWeeks";
import {
  useCreateWorkout,
  useDeleteWorkout,
  useImportGpxWorkout,
  useWorkoutDetails,
  useWorkouts,
} from "../hooks/useWorkouts";
import {
  formatDateTime,
  formatDuration,
  formatPace,
  toDateTimeLocalValue,
} from "../utils/format";

const workoutTypes: WorkoutType[] = ["Easy", "Quality", "Long", "Recovery"];

type NoticeKind = "success" | "error";

interface Notice {
  kind: NoticeKind;
  message: string;
}

export function WorkoutsPage() {
  const workouts = useWorkouts();
  const trainingWeeks = useTrainingWeeks();
  const activeGoal = useActiveRunningGoal();

  const [selectedWorkoutId, setSelectedWorkoutId] = useState<number | null>(
    null,
  );

  const createWorkout = useCreateWorkout();
  const deleteWorkout = useDeleteWorkout();
  const importGpxWorkout = useImportGpxWorkout();
  const workoutDetails = useWorkoutDetails(selectedWorkoutId);

  const [isAddWorkoutOpen, setIsAddWorkoutOpen] = useState(false);
  const [isImportGpxOpen, setIsImportGpxOpen] = useState(false);

  const [startedAt, setStartedAt] = useState(toDateTimeLocalValue());
  const [distanceKm, setDistanceKm] = useState("5");
  const [hours, setHours] = useState("0");
  const [minutes, setMinutes] = useState("30");
  const [seconds, setSeconds] = useState("0");
  const [rpe, setRpe] = useState("5");
  const [type, setType] = useState<WorkoutType>("Easy");
  const [plannedSessionId, setPlannedSessionId] = useState("");
  const [notes, setNotes] = useState("");

  const [gpxFile, setGpxFile] = useState<File | null>(null);
  const [gpxInputKey, setGpxInputKey] = useState(0);
  const [importRpe, setImportRpe] = useState("5");
  const [importType, setImportType] = useState<WorkoutType>("Easy");
  const [importNotes, setImportNotes] = useState("");

  const [hasTriedCreateSubmit, setHasTriedCreateSubmit] = useState(false);
  const [hasTriedImportSubmit, setHasTriedImportSubmit] = useState(false);
  const [createNotice, setCreateNotice] = useState<Notice | null>(null);
  const [importNotice, setImportNotice] = useState<Notice | null>(null);

  const [workoutIdPendingDelete, setWorkoutIdPendingDelete] = useState<
    number | null
  >(null);

  useEffect(() => {
    if (createNotice === null) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setCreateNotice(null);
    }, 3500);

    return () => window.clearTimeout(timeoutId);
  }, [createNotice]);

  useEffect(() => {
    if (importNotice === null) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setImportNotice(null);
    }, 4500);

    return () => window.clearTimeout(timeoutId);
  }, [importNotice]);

  const availablePlannedSessions = useMemo(() => {
    return getPlannedSessionsForStartedAtWeek(
      trainingWeeks.data ?? [],
      startedAt,
    );
  }, [trainingWeeks.data, startedAt]);

  const sortedWorkouts = useMemo(() => {
    return (workouts.data ?? [])
      .slice()
      .sort(
        (a, b) =>
          new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime(),
      );
  }, [workouts.data]);

  const formValidationError = getWorkoutFormValidationError({
    startedAt,
    distanceKm,
    hours,
    minutes,
    seconds,
    rpe,
  });

  const gpxImportValidationError = getGpxImportValidationError({
    file: gpxFile,
    rpe: importRpe,
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
    setHasTriedCreateSubmit(true);

    if (formValidationError !== null) {
      return;
    }

    const durationSeconds =
      Number(hours) * 3600 + Number(minutes) * 60 + Number(seconds);

    createWorkout.mutate(
      {
        startedAt: new Date(startedAt).toISOString(),
        distanceKm: Number(distanceKm),
        durationSeconds,
        rpe: Number(rpe),
        type,
        notes: notes.trim() === "" ? null : notes.trim(),
        plannedSessionId:
          plannedSessionId.trim() === "" ? null : Number(plannedSessionId),
      },
      {
        onSuccess: (createdWorkout) => {
          setCreateNotice({
            kind: "success",
            message: "Workout saved.",
          });
          setHasTriedCreateSubmit(false);
          setSelectedWorkoutId(createdWorkout.id);
        },
        onError: (error) => {
          setCreateNotice({
            kind: "error",
            message: getErrorMessage(error),
          });
        },
      },
    );
  }

  function handleGpxFileChange(file: File | null) {
    setGpxFile(file);
  }


  function handleImportGpx() {
    setHasTriedImportSubmit(true);

    if (gpxImportValidationError !== null || gpxFile === null) {
      return;
    }

    importGpxWorkout.mutate(
      {
        file: gpxFile,
        plannedSessionId: null,
        workoutType: importType,
        rpe: Number(importRpe),
        notes: importNotes.trim() === "" ? null : importNotes.trim(),
      },
      {
        onSuccess: (createdWorkout) => {
          setGpxFile(null);
          setGpxInputKey((value) => value + 1);
          setImportRpe("5");
          setImportType("Easy");
          setImportNotes("");
          setHasTriedImportSubmit(false);

          setImportNotice({
            kind: "success",
            message: "GPX workout imported.",
          });

          setSelectedWorkoutId(createdWorkout.id);
        },
        onError: (error) => {
          setImportNotice({
            kind: "error",
            message: getErrorMessage(error),
          });
        },
      },
    );
  }

  function confirmDeleteWorkout() {
    if (workoutIdPendingDelete === null) {
      return;
    }

    deleteWorkout.mutate(workoutIdPendingDelete, {
      onSuccess: () => {
        if (selectedWorkoutId === workoutIdPendingDelete) {
          setSelectedWorkoutId(null);
        }

        setWorkoutIdPendingDelete(null);
      },
    });
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Workouts</h2>
          <p>Add completed runs, import GPX files, and review workout details.</p>
        </div>
      </div>

      <section className="card collapsible-card">
      <button
        type="button"
        className="collapsible-header"
        onClick={() => setIsAddWorkoutOpen((value) => !value)}
      >
        <span>Add workout</span>
        <span className="collapsible-indicator">
          {isAddWorkoutOpen ? "−" : "+"}
        </span>
      </button>

        {isAddWorkoutOpen && (
          <div className="collapsible-content">
            <div className="form-grid">
              <label className="field">
                <span>Started at</span>
                <input
                  type="datetime-local"
                  max={toDateTimeLocalValue()}
                  value={startedAt}
                  onChange={(event) =>
                    handleStartedAtChange(event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>Link to planned session, optional</span>
                <select
                  value={plannedSessionId}
                  onChange={(event) =>
                    handlePlannedSessionChange(event.target.value)
                  }
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
                  onChange={(event) =>
                    setType(event.target.value as WorkoutType)
                  }
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

            {hasTriedCreateSubmit && formValidationError && (
              <NoticeBlock kind="error" message={formValidationError} />
            )}

            <div className="actions">
              <button
                type="button"
                onClick={handleCreate}
                disabled={createWorkout.isPending}
              >
                {createWorkout.isPending ? "Saving..." : "Add workout"}
              </button>
            </div>

            {createNotice && (
              <NoticeBlock
                kind={createNotice.kind}
                message={createNotice.message}
              />
            )}

            {goalAchievementMessage && (
              <div className="goal-notification">
                <strong>Goal matched.</strong>
                <p>{goalAchievementMessage}</p>
              </div>
            )}
          </div>
        )}
      </section>

      <section className="card collapsible-card">
      <button
        type="button"
        className="collapsible-header"
        onClick={() => setIsImportGpxOpen((value) => !value)}
      >
        <span>Import GPX</span>
        <span className="collapsible-indicator">
          {isImportGpxOpen ? "−" : "+"}
        </span>
      </button>

        {isImportGpxOpen && (
          <div className="collapsible-content">
            <div className="form-grid">
              <label className="field">
                <span>GPX file</span>
                <input
                  key={gpxInputKey}
                  type="file"
                  accept=".gpx,application/gpx+xml,application/xml,text/xml"
                  onChange={(event) =>
                    handleGpxFileChange(event.target.files?.[0] ?? null)
                  }
                />
              </label>

              <label className="field">
                <span>RPE</span>
                <input
                  type="number"
                  min="1"
                  max="10"
                  value={importRpe}
                  onChange={(event) => setImportRpe(event.target.value)}
                />
              </label>

              <label className="field">
                <span>Type</span>
                <select
                  value={importType}
                  onChange={(event) =>
                    setImportType(event.target.value as WorkoutType)
                  }
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
                value={importNotes}
                onChange={(event) => setImportNotes(event.target.value)}
              />
            </label>

            {hasTriedImportSubmit && gpxImportValidationError && (
              <NoticeBlock kind="error" message={gpxImportValidationError} />
            )}

            <div className="actions">
              <button
                type="button"
                onClick={handleImportGpx}
                disabled={importGpxWorkout.isPending}
              >
                {importGpxWorkout.isPending ? "Importing..." : "Import GPX"}
              </button>
            </div>

            {importNotice && (
              <NoticeBlock
                kind={importNotice.kind}
                message={importNotice.message}
              />
            )}
          </div>
        )}
      </section>

      <section className="card">
        <h3>Workout history</h3>

        {workouts.isLoading && <p className="muted">Loading workouts...</p>}

        {workouts.error && <p className="error">{workouts.error.message}</p>}

        {!workouts.isLoading && sortedWorkouts.length === 0 && (
          <p className="muted">No workouts yet.</p>
        )}

        <div className="scroll-panel">
          <div className="list">
            {sortedWorkouts.map((workout) => (
              <article key={workout.id} className="workout-history-item">
                <div className="workout-history-main">
                  <div>
                    <h4>
                      {workout.type}
                      <span className={getSourceBadgeClassName(workout.source)}>
                        {workout.source}
                      </span>
                    </h4>

                    <p className="muted">
                      {formatDateTime(workout.startedAt)}
                      {workout.plannedSessionId
                        ? " · Linked planned session"
                        : ""}
                    </p>
                  </div>

                  <div className="workout-history-metrics">
                    <span>{workout.distanceKm} km</span>
                    <span>{formatDuration(workout.durationSeconds)}</span>
                    <span>{formatPace(workout.averagePaceSecondsPerKm)}</span>
                    <span>RPE {workout.rpe}</span>
                  </div>
                </div>

                {workout.notes && <p className="muted">{workout.notes}</p>}

                <div className="actions-inline">
                  <button
                    type="button"
                    className="secondary-button"
                    onClick={() => setSelectedWorkoutId(workout.id)}
                  >
                    Details
                  </button>

                  <button
                    type="button"
                    className="danger-button"
                    onClick={() => setWorkoutIdPendingDelete(workout.id)}
                    disabled={deleteWorkout.isPending}
                  >
                    Delete
                  </button>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>

      {selectedWorkoutId !== null && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal-panel modal-panel-large">
            {workoutDetails.isLoading && (
              <section className="card modal-card">
                <h3>Workout details</h3>
                <p className="muted">Loading workout details...</p>
              </section>
            )}

            {workoutDetails.error && (
              <section className="card modal-card">
                <h3>Workout details</h3>
                <NoticeBlock
                  kind="error"
                  message={workoutDetails.error.message}
                />
                <button
                  type="button"
                  className="secondary-button"
                  onClick={() => setSelectedWorkoutId(null)}
                >
                  Close
                </button>
              </section>
            )}

            {workoutDetails.data && (
              <WorkoutDetailsPanel
              key={workoutDetails.data.summary.id}
              details={workoutDetails.data}
              onClose={() => setSelectedWorkoutId(null)}
            />
            )}
          </div>
        </div>
      )}

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

interface NoticeBlockProps {
  kind: NoticeKind;
  message: string;
}

function NoticeBlock({ kind, message }: NoticeBlockProps) {
  return (
    <div className={`notice-block notice-block-${kind}`}>
      <p>{message}</p>
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

interface GpxImportFormValues {
  file: File | null;
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

  const rpeValue = Number(values.rpe);

  if (!Number.isFinite(rpeValue) || rpeValue < 1 || rpeValue > 10) {
    return "RPE must be between 1 and 10.";
  }

  return null;
}

function getGpxImportValidationError(
  values: GpxImportFormValues,
): string | null {
  if (values.file === null) {
    return "GPX file is required.";
  }

  if (!values.file.name.toLowerCase().endsWith(".gpx")) {
    return "Only .gpx files are supported.";
  }

  if (values.file.size <= 0) {
    return "GPX file is empty.";
  }

  const rpeValue = Number(values.rpe);

  if (!Number.isFinite(rpeValue) || rpeValue < 1 || rpeValue > 10) {
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

  const matchingWeek = trainingWeeks.find((week) => {
    const weekStart = parseLocalDate(week.weekStartDate);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 7);

    return selectedDate >= weekStart && selectedDate < weekEnd;
  });

  if (!matchingWeek) {
    return [];
  }

  return matchingWeek.plannedSessions
    .filter(isImportablePlannedSession)
    .sort((a, b) => {
      const statusOrder =
        getPlannedSessionStatusOrder(a.status) -
        getPlannedSessionStatusOrder(b.status);

      if (statusOrder !== 0) {
        return statusOrder;
      }

      return a.sortOrder - b.sortOrder;
    });
}

function parseLocalDate(dateKey: string): Date {
  const [year, month, day] = dateKey.split("-").map(Number);
  return new Date(year, month - 1, day, 0, 0, 0, 0);
}

function isImportablePlannedSession(session: PlannedSessionDto): boolean {
  return (
    session.status === "Unscheduled" ||
    session.status === "Scheduled" ||
    session.status === "Skipped"
  );
}

function getPlannedSessionStatusOrder(
  status: PlannedSessionDto["status"],
): number {
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

function formatPlannedSessionOption(session: PlannedSessionDto): string {
  const scheduledLabel = session.scheduledFor
    ? formatDateTime(session.scheduledFor)
    : "Unscheduled";

  return `${session.status} · ${scheduledLabel} · ${session.type} · ${formatDuration(
    session.targetDurationSeconds,
  )} · ${session.targetDistanceKm ?? "-"} km`;
}

function getSourceBadgeClassName(source: WorkoutDto["source"]): string {
  return `source-badge source-badge-${String(source).toLowerCase()}`;
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

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Request failed.";
}