import { useMemo, useState } from "react";
import type {
  PlannedSessionDto,
  TrainingWeekDto,
  WorkoutDto,
} from "../api/types";
import {
  useSchedulePlannedSession,
  useSkipPlannedSession,
} from "../hooks/usePlannedSessions";
import {
  useGenerateTrainingWeek,
  useTrainingWeeks,
} from "../hooks/useTrainingWeeks";
import { useWorkouts } from "../hooks/useWorkouts";
import {
  formatDate,
  formatDateTime,
  formatDuration,
  formatPace,
  toDateTimeLocalValue,
  toDateTimeLocalValueFromIso,
  toLocalDateKey,
} from "../utils/format";

export function TrainingPlanPage() {
  const [weekStartDate, setWeekStartDate] = useState("2026-06-01");
  const [calendarMonth, setCalendarMonth] = useState("2026-06");
  const [selectedWeekId, setSelectedWeekId] = useState<number | null>(null);

  const trainingWeeks = useTrainingWeeks();
  const workouts = useWorkouts();
  const generateTrainingWeek = useGenerateTrainingWeek();

  const weeks = trainingWeeks.data ?? [];

  const selectedWeek = useMemo(() => {
    if (weeks.length === 0) {
      return null;
    }

    return weeks.find((week) => week.id === selectedWeekId) ?? weeks[0];
  }, [weeks, selectedWeekId]);

  const selectedWeekIndex = selectedWeek
    ? weeks.findIndex((week) => week.id === selectedWeek.id)
    : -1;

  function handleGenerate() {
    generateTrainingWeek.mutate(
      { weekStartDate },
      {
        onSuccess: (week) => {
          setSelectedWeekId(week.id);
        },
      },
    );
  }

  function selectPreviousWeek() {
    if (selectedWeekIndex <= 0) {
      return;
    }

    setSelectedWeekId(weeks[selectedWeekIndex - 1].id);
  }

  function selectNextWeek() {
    if (selectedWeekIndex < 0 || selectedWeekIndex >= weeks.length - 1) {
      return;
    }

    setSelectedWeekId(weeks[selectedWeekIndex + 1].id);
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Training Plan</h2>
          <p>Generate, schedule and review adaptive weekly running plans.</p>
        </div>
      </div>

      <section className="card">
        <h3>Generate training week</h3>
        <p className="muted">
          The system analyzes recent workouts, skipped sessions, runner profile
          and active goal.
        </p>

        <div className="form-grid">
          <label className="field">
            <span>Week start date</span>
            <input
              type="date"
              value={weekStartDate}
              onChange={(event) => setWeekStartDate(event.target.value)}
            />
          </label>
        </div>

        <div className="actions">
          <button
            type="button"
            onClick={handleGenerate}
            disabled={generateTrainingWeek.isPending}
          >
            {generateTrainingWeek.isPending ? "Generating..." : "Generate"}
          </button>
        </div>

        {generateTrainingWeek.data && (
          <div className="notice-block notice-block-success">
            <p>
              Generated week:{" "}
              {formatDate(generateTrainingWeek.data.weekStartDate)} · Mode:{" "}
              {generateTrainingWeek.data.adjustmentMode} · Sessions:{" "}
              {generateTrainingWeek.data.targetWorkoutCount} · Total duration:{" "}
              {formatDuration(
                generateTrainingWeek.data.targetWeekDurationSeconds,
              )}
            </p>
          </div>
        )}

        {generateTrainingWeek.error && (
          <div className="notice-block notice-block-error">
            <p>{generateTrainingWeek.error.message}</p>
          </div>
        )}
      </section>

      <section className="card">
        <h3>Generated week</h3>

        <div className="week-pager">
          <div>
            <p className="muted">
              Select a generated week instead of rendering all weeks as one long
              list.
            </p>
          </div>

          <div className="week-pager-actions">
            <button
              type="button"
              className="secondary-button"
              onClick={selectPreviousWeek}
              disabled={selectedWeekIndex <= 0}
            >
              Previous
            </button>

            <select
              value={selectedWeek?.id ?? ""}
              onChange={(event) =>
                setSelectedWeekId(Number(event.target.value))
              }
              disabled={weeks.length === 0}
            >
              {weeks.map((week) => (
                <option key={week.id} value={week.id}>
                  {formatDate(week.weekStartDate)} · {week.adjustmentMode}
                </option>
              ))}
            </select>

            <button
              type="button"
              className="secondary-button"
              onClick={selectNextWeek}
              disabled={
                selectedWeekIndex < 0 || selectedWeekIndex >= weeks.length - 1
              }
            >
              Next
            </button>
          </div>
        </div>

        {trainingWeeks.isLoading && (
          <p className="muted">Loading training weeks...</p>
        )}

        {trainingWeeks.error && (
          <div className="notice-block notice-block-error">
            <p>{trainingWeeks.error.message}</p>
          </div>
        )}

        {weeks.length === 0 && !trainingWeeks.isLoading && (
          <p className="muted">No generated weeks yet.</p>
        )}

        {selectedWeek && <TrainingWeekCard week={selectedWeek} />}
      </section>

      <TrainingCalendar
        weeks={weeks}
        workouts={workouts.data ?? []}
        calendarMonth={calendarMonth}
        onCalendarMonthChange={setCalendarMonth}
      />
    </div>
  );
}

function TrainingWeekCard({ week }: { week: TrainingWeekDto }) {
  const sortedSessions = [...week.plannedSessions].sort(
    (a, b) => a.sortOrder - b.sortOrder,
  );

  return (
    <div className="week-card">
      <div className="week-header">
        <div>
          <h3>Week of {formatDate(week.weekStartDate)}</h3>
          <p className="muted">
            {week.adjustmentMode} · {week.targetWorkoutCount} sessions ·{" "}
            {formatDuration(week.targetWeekDurationSeconds)}
          </p>
        </div>

        <span className={`status status-${week.status.toLowerCase()}`}>
          {week.status}
        </span>
      </div>

      {week.explanation && <p>{week.explanation}</p>}

      <div className="sessions">
        {sortedSessions.map((session) => (
          <PlannedSessionCard key={session.id} session={session} />
        ))}
      </div>
    </div>
  );
}

function PlannedSessionCard({ session }: { session: PlannedSessionDto }) {
  const [scheduledFor, setScheduledFor] = useState(
    toDateTimeLocalValueFromIso(session.scheduledFor),
  );

  const scheduleSession = useSchedulePlannedSession();
  const skipSession = useSkipPlannedSession();

  const isCompleted = session.status === "Completed";
  const isSkipped = session.status === "Skipped";
  const canEditSchedule = !isCompleted && !isSkipped;

  const minimumScheduleValue = toDateTimeLocalValue();
  const scheduleIsPast =
    canEditSchedule && scheduledFor !== "" && new Date(scheduledFor) < new Date();

  function handleSchedule() {
    if (!scheduledFor || scheduleIsPast) {
      return;
    }

    scheduleSession.mutate({
      id: session.id,
      request: {
        scheduledFor: new Date(scheduledFor).toISOString(),
      },
    });
  }

  function handleSkip() {
    skipSession.mutate(session.id);
  }

  return (
    <article className={`session-card session-type-${session.type.toLowerCase()}`}>
      <div className="session-header">
        <div>
          <h4>
            {session.sortOrder}. {session.type}
          </h4>
          <p className="muted">{session.intensity}</p>
        </div>

        <span className={`status status-${session.status.toLowerCase()}`}>
          {session.status}
        </span>
      </div>

      <dl className="session-metrics">
        <div>
          <dt>Duration</dt>
          <dd>{formatDuration(session.targetDurationSeconds)}</dd>
        </div>

        <div>
          <dt>Distance</dt>
          <dd>{session.targetDistanceKm ?? "-"} km</dd>
        </div>

        <div>
          <dt>Pace</dt>
          <dd>{formatPace(session.targetPaceSecondsPerKm)}</dd>
        </div>

        <div>
          <dt>Scheduled</dt>
          <dd>{formatDateTime(session.scheduledFor)}</dd>
        </div>
      </dl>

      {session.notes && <p>{session.notes}</p>}

      {session.reason && <p className="reason">{session.reason}</p>}

      <div className="form-grid">
        <label className="field">
          <span>Schedule date and time</span>
          <input
            type="datetime-local"
            min={minimumScheduleValue}
            value={scheduledFor}
            onChange={(event) => setScheduledFor(event.target.value)}
            disabled={!canEditSchedule}
          />
        </label>
      </div>

      <div className="session-actions">
        <button
          type="button"
          onClick={handleSchedule}
          disabled={
            !canEditSchedule || scheduleSession.isPending || scheduleIsPast
          }
        >
          Schedule
        </button>

        <button
          type="button"
          className="secondary-button"
          onClick={handleSkip}
          disabled={!canEditSchedule || skipSession.isPending}
        >
          Skip
        </button>
      </div>

      {scheduleIsPast && canEditSchedule && (
        <div className="notice-block notice-block-error">
          <p>Planned session cannot be scheduled in the past.</p>
        </div>
      )}

      {scheduleSession.error && (
        <div className="notice-block notice-block-error">
          <p>{scheduleSession.error.message}</p>
        </div>
      )}

      {skipSession.error && (
        <div className="notice-block notice-block-error">
          <p>{skipSession.error.message}</p>
        </div>
      )}
    </article>
  );
}

interface TrainingCalendarProps {
  weeks: TrainingWeekDto[];
  workouts: WorkoutDto[];
  calendarMonth: string;
  onCalendarMonthChange: (month: string) => void;
}

function TrainingCalendar({
  weeks,
  workouts,
  calendarMonth,
  onCalendarMonthChange,
}: TrainingCalendarProps) {
  const calendarDays = useMemo(
    () => buildCalendarDays(calendarMonth),
    [calendarMonth],
  );

  const completedPlannedSessionIds = useMemo(() => {
    return new Set(
      workouts
        .map((workout) => workout.plannedSessionId)
        .filter((id): id is number => id !== null),
    );
  }, [workouts]);

  const plannedSessionsByDate = useMemo(() => {
    const result = new Map<string, PlannedSessionDto[]>();

    for (const week of weeks) {
      for (const session of week.plannedSessions) {
        if (!session.scheduledFor) {
          continue;
        }

        if (
          session.status === "Completed" &&
          completedPlannedSessionIds.has(session.id)
        ) {
          continue;
        }

        const dateKey = toLocalDateKey(new Date(session.scheduledFor));
        const existing = result.get(dateKey) ?? [];

        existing.push(session);
        result.set(dateKey, existing);
      }
    }

    for (const sessions of result.values()) {
      sessions.sort((a, b) => {
        return (
          new Date(a.scheduledFor!).getTime() -
          new Date(b.scheduledFor!).getTime()
        );
      });
    }

    return result;
  }, [completedPlannedSessionIds, weeks]);

  const workoutsByDate = useMemo(() => {
    const result = new Map<string, WorkoutDto[]>();

    for (const workout of workouts) {
      const dateKey = toLocalDateKey(new Date(workout.startedAt));
      const existing = result.get(dateKey) ?? [];

      existing.push(workout);
      result.set(dateKey, existing);
    }

    for (const dayWorkouts of result.values()) {
      dayWorkouts.sort((a, b) => {
        return (
          new Date(a.startedAt).getTime() - new Date(b.startedAt).getTime()
        );
      });
    }

    return result;
  }, [workouts]);

  function goToPreviousMonth() {
    onCalendarMonthChange(shiftMonth(calendarMonth, -1));
  }

  function goToNextMonth() {
    onCalendarMonthChange(shiftMonth(calendarMonth, 1));
  }

  return (
    <section className="card calendar-card">
      <div className="calendar-toolbar">
        <div>
          <h3>Monthly training calendar</h3>
          <p className="muted">
            Planned sessions are shown on their scheduled date. Completed
            workouts are shown on the date they were actually performed.
          </p>
        </div>

        <div className="month-actions">
          <button
            type="button"
            className="secondary-button"
            onClick={goToPreviousMonth}
          >
            Previous
          </button>

          <label className="field">
            <span>Month</span>
            <input
              type="month"
              value={calendarMonth}
              onChange={(event) => onCalendarMonthChange(event.target.value)}
            />
          </label>

          <button
            type="button"
            className="secondary-button"
            onClick={goToNextMonth}
          >
            Next
          </button>
        </div>
      </div>

      <div className="calendar-grid">
        {["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map((day) => (
          <div key={day} className="calendar-weekday">
            {day}
          </div>
        ))}

        {calendarDays.map((day, index) => {
          if (day === null) {
            return (
              <div key={`empty-${index}`} className="calendar-day empty" />
            );
          }

          const sessions = plannedSessionsByDate.get(day.dateKey) ?? [];
          const dayWorkouts = workoutsByDate.get(day.dateKey) ?? [];

          return (
            <div key={day.dateKey} className="calendar-day">
              <div className="calendar-date">{day.dayOfMonth}</div>

              {sessions.map((session) => (
                <CalendarPlannedSession key={session.id} session={session} />
              ))}

              {dayWorkouts.map((workout) => (
                <CalendarWorkout key={workout.id} workout={workout} />
              ))}
            </div>
          );
        })}
      </div>
    </section>
  );
}

function CalendarPlannedSession({
  session,
}: {
  session: PlannedSessionDto;
}) {
  return (
    <div
      className={`calendar-session calendar-session-${session.status.toLowerCase()}`}
      title={formatPlannedSessionTooltip(session)}
    >
      <strong>{session.type}</strong>
      <span>{session.status}</span>
      <span>{formatDuration(session.targetDurationSeconds)}</span>
      <span>{session.targetDistanceKm ?? "-"} km</span>
    </div>
  );
}

function CalendarWorkout({ workout }: { workout: WorkoutDto }) {
  return (
    <div
      className={`calendar-session calendar-workout calendar-workout-${String(
        workout.source,
      ).toLowerCase()}`}
      title={formatWorkoutTooltip(workout)}
    >
      <strong>{workout.type}</strong>
      <span>Done · {workout.distanceKm} km</span>
      <span>{formatPace(workout.averagePaceSecondsPerKm)}</span>
    </div>
  );
}

interface CalendarDay {
  dateKey: string;
  dayOfMonth: number;
}

function buildCalendarDays(monthValue: string): Array<CalendarDay | null> {
  const [year, month] = monthValue.split("-").map(Number);
  const firstDay = new Date(year, month - 1, 1);
  const daysInMonth = new Date(year, month, 0).getDate();
  const mondayBasedStartIndex = (firstDay.getDay() + 6) % 7;

  const days: Array<CalendarDay | null> = [];

  for (let index = 0; index < mondayBasedStartIndex; index += 1) {
    days.push(null);
  }

  for (let day = 1; day <= daysInMonth; day += 1) {
    const date = new Date(year, month - 1, day);

    days.push({
      dateKey: toLocalDateKey(date),
      dayOfMonth: day,
    });
  }

  return days;
}

function shiftMonth(monthValue: string, offset: number): string {
  const [year, month] = monthValue.split("-").map(Number);
  const date = new Date(year, month - 1 + offset, 1);

  const shiftedYear = date.getFullYear();
  const shiftedMonth = String(date.getMonth() + 1).padStart(2, "0");

  return `${shiftedYear}-${shiftedMonth}`;
}

function formatPlannedSessionTooltip(session: PlannedSessionDto): string {
  const parts = [
    `Planned: ${session.type}`,
    `Status: ${session.status}`,
    `Duration: ${formatDuration(session.targetDurationSeconds)}`,
    `Distance: ${session.targetDistanceKm ?? "-"} km`,
  ];

  if (session.targetPaceSecondsPerKm !== null) {
    parts.push(`Pace: ${formatPace(session.targetPaceSecondsPerKm)}`);
  }

  if (session.scheduledFor !== null) {
    parts.push(`Scheduled: ${formatDateTime(session.scheduledFor)}`);
  }

  return parts.join("\n");
}

function formatWorkoutTooltip(workout: WorkoutDto): string {
  const parts = [
    `Completed: ${workout.type}`,
    `Source: ${workout.source}`,
    `Started: ${formatDateTime(workout.startedAt)}`,
    `Distance: ${workout.distanceKm} km`,
    `Duration: ${formatDuration(workout.durationSeconds)}`,
    `Pace: ${formatPace(workout.averagePaceSecondsPerKm)}`,
    `RPE: ${workout.rpe}`,
  ];

  if (workout.plannedSessionId !== null) {
    parts.push("Linked to planned session");
  }

  return parts.join("\n");
}