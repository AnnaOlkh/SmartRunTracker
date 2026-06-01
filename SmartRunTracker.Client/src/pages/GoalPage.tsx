import { useMemo, useState } from "react";
import type { RunningGoalDto } from "../api/types";
import {
  useActivateRunningGoal,
  useActiveRunningGoal,
  useCreateRunningGoal,
  useDeleteRunningGoal,
  useRunningGoals,
} from "../hooks/useRunningGoals";
import { formatDate, formatPace, toLocalDateKey } from "../utils/format";

export function GoalPage() {
  const activeGoal = useActiveRunningGoal();
  const runningGoals = useRunningGoals();
  const createRunningGoal = useCreateRunningGoal();
  const activateRunningGoal = useActivateRunningGoal();
  const deleteRunningGoal = useDeleteRunningGoal();

  const today = toLocalDateKey(new Date());

  const [targetDistanceKm, setTargetDistanceKm] = useState("10");
  const [targetPace, setTargetPace] = useState("6:00");
  const [goalDate, setGoalDate] = useState("2026-08-01");

  const parsedPace = parsePace(targetPace);

  const validationError = useMemo(() => {
    if (Number(targetDistanceKm) <= 0) {
      return "Target distance must be greater than zero.";
    }

    if (parsedPace === null) {
      return "Target pace must use format mm:ss, for example 6:00.";
    }

    if (goalDate && goalDate < today) {
      return "Goal date cannot be in the past.";
    }

    const duplicateExists = runningGoals.data?.some((goal) => {
      return (
        goal.targetDistanceKm === Number(targetDistanceKm)
        && goal.targetPaceSecondsPerKm === parsedPace
        && goal.goalDate === (goalDate.trim() === "" ? null : goalDate)
      );
    });

    if (duplicateExists) {
      return "This running goal already exists.";
    }

    return null;
  }, [goalDate, parsedPace, runningGoals.data, targetDistanceKm, today]);

  function handleCreate() {
    if (validationError !== null || parsedPace === null) {
      return;
    }

    createRunningGoal.mutate({
      targetDistanceKm: Number(targetDistanceKm),
      targetPaceSecondsPerKm: parsedPace,
      goalDate: goalDate.trim() === "" ? null : goalDate,
    });
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Running Goal</h2>
          <p>Set the active goal used by the adaptive training planner.</p>
        </div>
      </div>

      <section className="card">
        <h3>Active goal</h3>

        {activeGoal.data ? (
          <>
            <p className="metric">{activeGoal.data.targetDistanceKm} km</p>
            <p className="muted">
              Target pace: {formatPace(activeGoal.data.targetPaceSecondsPerKm)}
            </p>
            <p className="muted">
              Goal date: {formatDate(activeGoal.data.goalDate)}
            </p>
          </>
        ) : (
          <p className="muted">No active goal yet.</p>
        )}
      </section>

      <section className="card">
        <h3>Create new goal</h3>

        <div className="form-grid">
          <label className="field">
            <span>Target distance, km</span>
            <input
              type="number"
              min="0"
              step="0.01"
              value={targetDistanceKm}
              onChange={(event) => setTargetDistanceKm(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Target pace, min/km</span>
            <input
              type="text"
              placeholder="6:00"
              value={targetPace}
              onChange={(event) => setTargetPace(event.target.value)}
            />
          </label>

          <label className="field">
            <span>Goal date</span>
            <input
              type="date"
              min={today}
              value={goalDate}
              onChange={(event) => setGoalDate(event.target.value)}
            />
          </label>
        </div>

        {validationError && <p className="error">{validationError}</p>}

        <div className="actions">
          <button
            type="button"
            onClick={handleCreate}
            disabled={createRunningGoal.isPending || validationError !== null}
          >
            {createRunningGoal.isPending ? "Saving..." : "Create goal"}
          </button>
        </div>

        {createRunningGoal.error && (
          <p className="error">{createRunningGoal.error.message}</p>
        )}

        {createRunningGoal.data && <p className="success">Goal created.</p>}
      </section>

      <section className="card">
        <h3>Goal history</h3>

        <div className="scroll-panel">
          <div className="list">
            {runningGoals.data?.map((goal) => (
              <GoalHistoryItem
                key={goal.id}
                goal={goal}
                onActivate={() => activateRunningGoal.mutate(goal.id)}
                onDelete={() => deleteRunningGoal.mutate(goal.id)}
                activatePending={activateRunningGoal.isPending}
                deletePending={deleteRunningGoal.isPending}
              />
            ))}
          </div>
        </div>

        {activateRunningGoal.error && (
          <p className="error">{activateRunningGoal.error.message}</p>
        )}

        {deleteRunningGoal.error && (
          <p className="error">{deleteRunningGoal.error.message}</p>
        )}
      </section>
    </div>
  );
}

function GoalHistoryItem({
  goal,
  onActivate,
  onDelete,
  activatePending,
  deletePending,
}: {
  goal: RunningGoalDto;
  onActivate: () => void;
  onDelete: () => void;
  activatePending: boolean;
  deletePending: boolean;
}) {
  return (
    <article className="list-item">
      <div className="list-item-header">
        <div>
          <h4>
            {goal.targetDistanceKm} km · {formatPace(goal.targetPaceSecondsPerKm)}
          </h4>
          <p className="muted">Goal date: {formatDate(goal.goalDate)}</p>
        </div>

        <span
          className={
            goal.isActive
              ? "status status-completed"
              : "status status-unscheduled"
          }
        >
          {goal.isActive ? "Active" : "Inactive"}
        </span>
      </div>

      <div className="actions">
        {!goal.isActive && (
          <button
            type="button"
            className="secondary-button"
            onClick={onActivate}
            disabled={activatePending}
          >
            Make active
          </button>
        )}

        <button
          type="button"
          className="danger-button"
          onClick={onDelete}
          disabled={deletePending}
        >
          Delete
        </button>
      </div>
    </article>
  );
}

function parsePace(value: string): number | null {
  const match = /^(\d{1,2}):([0-5]\d)$/.exec(value.trim());

  if (!match) {
    return null;
  }

  return Number(match[1]) * 60 + Number(match[2]);
}