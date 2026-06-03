import { useEffect, useMemo, useState } from "react";
import type { RunningGoalDto } from "../api/types";
import {
  useActivateRunningGoal,
  useActiveRunningGoal,
  useCreateRunningGoal,
  useDeleteRunningGoal,
  useRunningGoals,
} from "../hooks/useRunningGoals";
import { formatDate, formatPace, toLocalDateKey } from "../utils/format";
import { ApiError } from "../api/client";

type NoticeKind = "success" | "error";

interface Notice {
  kind: NoticeKind;
  message: string;
}

export function GoalPage() {
  const activeGoal = useActiveRunningGoal();
  const runningGoals = useRunningGoals();
  const createRunningGoal = useCreateRunningGoal();
  const activateRunningGoal = useActivateRunningGoal();
  const deleteRunningGoal = useDeleteRunningGoal();

  const today = toLocalDateKey(new Date());

  const [isCreateGoalOpen, setIsCreateGoalOpen] = useState(false);
  const [hasTriedSubmit, setHasTriedSubmit] = useState(false);
  const [goalNotice, setGoalNotice] = useState<Notice | null>(null);

  const [targetDistanceKm, setTargetDistanceKm] = useState("10");
  const [targetPace, setTargetPace] = useState("6:00");
  const [goalDate, setGoalDate] = useState("2026-08-01");

  const parsedPace = parsePace(targetPace);

  const activeGoalError =
  activeGoal.error instanceof ApiError && activeGoal.error.status !== 404
    ? activeGoal.error.message
    : null;

  useEffect(() => {
    if (goalNotice === null) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setGoalNotice(null);
    }, 3500);

    return () => window.clearTimeout(timeoutId);
  }, [goalNotice]);

  const sortedGoals = useMemo(() => {
    return (runningGoals.data ?? [])
      .slice()
      .sort((a, b) => {
        if (a.isActive && !b.isActive) {
          return -1;
        }

        if (!a.isActive && b.isActive) {
          return 1;
        }

        if (a.goalDate && b.goalDate) {
          return (
            new Date(b.goalDate).getTime() - new Date(a.goalDate).getTime()
          );
        }

        if (a.goalDate) {
          return -1;
        }

        if (b.goalDate) {
          return 1;
        }

        return b.id - a.id;
      });
  }, [runningGoals.data]);

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
        goal.targetDistanceKm === Number(targetDistanceKm) &&
        goal.targetPaceSecondsPerKm === parsedPace &&
        goal.goalDate === (goalDate.trim() === "" ? null : goalDate)
      );
    });

    if (duplicateExists) {
      return "This running goal already exists.";
    }

    return null;
  }, [goalDate, parsedPace, runningGoals.data, targetDistanceKm, today]);

  function handleCreate() {
    setHasTriedSubmit(true);

    if (validationError !== null || parsedPace === null) {
      return;
    }

    createRunningGoal.mutate(
      {
        targetDistanceKm: Number(targetDistanceKm),
        targetPaceSecondsPerKm: parsedPace,
        goalDate: goalDate.trim() === "" ? null : goalDate,
      },
      {
        onSuccess: () => {
          setGoalNotice({
            kind: "success",
            message: "Goal created.",
          });

          setHasTriedSubmit(false);
        },
        onError: (error) => {
          setGoalNotice({
            kind: "error",
            message: getErrorMessage(error),
          });
        },
      },
    );
  }

  function handleActivate(goalId: number) {
    activateRunningGoal.mutate(goalId, {
      onSuccess: () => {
        setGoalNotice({
          kind: "success",
          message: "Goal activated.",
        });
      },
      onError: (error) => {
        setGoalNotice({
          kind: "error",
          message: getErrorMessage(error),
        });
      },
    });
  }

  function handleDelete(goalId: number) {
    deleteRunningGoal.mutate(goalId, {
      onSuccess: () => {
        setGoalNotice({
          kind: "success",
          message: "Goal deleted.",
        });
      },
      onError: (error) => {
        setGoalNotice({
          kind: "error",
          message: getErrorMessage(error),
        });
      },
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

        {activeGoal.isLoading && <p className="muted">Loading active goal...</p>}

        {activeGoalError && (
          <NoticeBlock kind="error" message={activeGoalError} />
        )}

        {activeGoal.data ? (
          <div className="stats-grid">
            <div className="stat-card">
              <span>Distance</span>
              <strong>{activeGoal.data.targetDistanceKm} km</strong>
            </div>

            <div className="stat-card">
              <span>Target pace</span>
              <strong>
                {formatPace(activeGoal.data.targetPaceSecondsPerKm)}
              </strong>
            </div>

            <div className="stat-card">
              <span>Goal date</span>
              <strong>{formatDate(activeGoal.data.goalDate)}</strong>
            </div>
          </div>
        ) : (
          !activeGoal.isLoading &&
          !activeGoalError && <p className="muted">No active goal yet.</p>
        )}
      </section>

      <section className="card collapsible-card">
        <button
          type="button"
          className="collapsible-header"
          onClick={() => setIsCreateGoalOpen((value) => !value)}
        >
          <span>Create new goal</span>
          <span className="collapsible-indicator">
            {isCreateGoalOpen ? "−" : "+"}
          </span>
        </button>

        {isCreateGoalOpen && (
          <div className="collapsible-content">
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

            {hasTriedSubmit && validationError && (
              <NoticeBlock kind="error" message={validationError} />
            )}

            <div className="actions">
              <button
                type="button"
                onClick={handleCreate}
                disabled={createRunningGoal.isPending}
              >
                {createRunningGoal.isPending ? "Saving..." : "Create goal"}
              </button>
            </div>

            {goalNotice && (
              <NoticeBlock kind={goalNotice.kind} message={goalNotice.message} />
            )}
          </div>
        )}
      </section>

      <section className="card">
        <h3>Goal history</h3>

        {runningGoals.isLoading && <p className="muted">Loading goals...</p>}

        {runningGoals.error && (
          <NoticeBlock kind="error" message={runningGoals.error.message} />
        )}

        {!runningGoals.isLoading && sortedGoals.length === 0 && (
          <p className="muted">No goals yet.</p>
        )}

        <div className="scroll-panel">
          <div className="list">
            {sortedGoals.map((goal) => (
              <GoalHistoryItem
                key={goal.id}
                goal={goal}
                onActivate={() => handleActivate(goal.id)}
                onDelete={() => handleDelete(goal.id)}
                activatePending={activateRunningGoal.isPending}
                deletePending={deleteRunningGoal.isPending}
              />
            ))}
          </div>
        </div>
      </section>
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
            {goal.targetDistanceKm} km ·{" "}
            {formatPace(goal.targetPaceSecondsPerKm)}
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

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Request failed.";
}