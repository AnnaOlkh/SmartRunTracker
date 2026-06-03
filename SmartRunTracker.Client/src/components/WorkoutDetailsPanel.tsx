import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type {
  PlannedSessionDto,
  WorkoutDetailsDto,
  WorkoutInsightDto,
  WorkoutSplitDto,
} from "../api/types";
import { workoutsApi } from "../api/workoutsApi";
import { formatDateTime, formatDuration, formatPace } from "../utils/format";
import { WorkoutRouteMap } from "./WorkoutRouteMap";

interface WorkoutDetailsPanelProps {
  details: WorkoutDetailsDto;
  onClose: () => void;
}

export function WorkoutDetailsPanel({
  details,
  onClose,
}: WorkoutDetailsPanelProps) {
  const summary = details.summary;
  const queryClient = useQueryClient();

  const [selectedPlannedSessionId, setSelectedPlannedSessionId] = useState("");

  const shouldShowLinkBlock = details.plannedSession === null;

  const availablePlannedSessions = useQuery({
    queryKey: ["workouts", summary.id, "available-planned-sessions"],
    queryFn: () => workoutsApi.getAvailablePlannedSessions(summary.id),
    enabled: shouldShowLinkBlock,
  });

  const linkPlannedSession = useMutation({
    mutationFn: () => {
      return workoutsApi.linkPlannedSession(summary.id, {
        plannedSessionId: Number(selectedPlannedSessionId),
      });
    },
    onSuccess: async (updatedDetails) => {
      queryClient.setQueryData(
        queryKeys.workoutDetails(summary.id),
        updatedDetails,
      );

      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.workouts }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.workoutDetails(summary.id),
        }),
        queryClient.invalidateQueries({
          queryKey: ["workouts", summary.id, "available-planned-sessions"],
        }),
      ]);
    },
  });

  return (
    <section className="card modal-card">
      <div className="list-item-header">
        <div>
          <h3>Workout details</h3>
          <p className="muted">
            {summary.source} · {summary.type} ·{" "}
            {formatDateTime(summary.startedAt)}
          </p>
        </div>

        <button type="button" className="secondary-button" onClick={onClose}>
          Close
        </button>
      </div>

      <div className="details-block">
        <h4>Result</h4>

        <div className="stats-grid">
          <div className="stat-card">
            <span>Type</span>
            <strong>{summary.type}</strong>
          </div>

          <div className="stat-card">
            <span>RPE</span>
            <strong>{summary.rpe}</strong>
          </div>

          <div className="stat-card">
            <span>Duration</span>
            <strong>{formatDuration(summary.durationSeconds)}</strong>
          </div>

          <div className="stat-card">
            <span>Distance</span>
            <strong>{summary.distanceKm} km</strong>
          </div>

          <div className="stat-card">
            <span>Average pace</span>
            <strong>{formatPace(summary.averagePaceSecondsPerKm)}</strong>
          </div>
        </div>
      </div>

      {summary.notes && (
        <div className="details-block">
          <h4>Notes</h4>
          <p>{summary.notes}</p>
        </div>
      )}

      {shouldShowLinkBlock && (
        <LinkPlannedSessionBlock
          availablePlannedSessions={availablePlannedSessions.data ?? []}
          isLoading={availablePlannedSessions.isLoading}
          error={availablePlannedSessions.error}
          selectedPlannedSessionId={selectedPlannedSessionId}
          onSelectedPlannedSessionIdChange={setSelectedPlannedSessionId}
          onLink={() => linkPlannedSession.mutate()}
          isLinking={linkPlannedSession.isPending}
          linkError={linkPlannedSession.error}
        />
      )}

      {details.plannedSession && (
        <PlannedSessionComparisonBlock details={details} />
      )}

      {details.hasRouteData && (
        <div className="details-block">
          <h4>Route</h4>
          <WorkoutRouteMap routePoints={details.routePoints} />
        </div>
      )}

      <SplitsBlock splits={details.splits} />

      <InsightsBlock insights={details.insights} />
    </section>
  );
}

interface LinkPlannedSessionBlockProps {
  availablePlannedSessions: PlannedSessionDto[];
  isLoading: boolean;
  error: unknown;
  selectedPlannedSessionId: string;
  onSelectedPlannedSessionIdChange: (value: string) => void;
  onLink: () => void;
  isLinking: boolean;
  linkError: unknown;
}

function LinkPlannedSessionBlock({
  availablePlannedSessions,
  isLoading,
  error,
  selectedPlannedSessionId,
  onSelectedPlannedSessionIdChange,
  onLink,
  isLinking,
  linkError,
}: LinkPlannedSessionBlockProps) {
  const canLink = selectedPlannedSessionId !== "" && !isLinking;

  const errorMessage = error ? getErrorMessage(error) : null;
  const linkErrorMessage = linkError ? getErrorMessage(linkError) : null;

  return (
    <div className="details-block">
      <h4>Link to planned session</h4>

      <p className="muted">
        This workout is not linked to a generated session yet. Select one of the
        planned sessions from the training week that contains this workout date.
      </p>

      {isLoading && <p className="muted">Loading planned sessions...</p>}

      {errorMessage ? (
        <div className="notice-block notice-block-error">
          <p>{errorMessage}</p>
        </div>
      ) : null}

      {!isLoading && !errorMessage && availablePlannedSessions.length === 0 && (
        <p className="muted">
          No generated training week was found for this workout date.
        </p>
      )}

        {!isLoading && !errorMessage && availablePlannedSessions.length > 0 && (
        <div className="form-grid">
          <label className="field">
            <span>Planned session</span>

            <select
              value={selectedPlannedSessionId}
              onChange={(event) =>
                onSelectedPlannedSessionIdChange(event.target.value)
              }
            >
              <option value="">Select planned session</option>

              {availablePlannedSessions.map((session) => (
                <option key={session.id} value={session.id}>
                  {formatPlannedSessionOption(session)}
                </option>
              ))}
            </select>
          </label>

          {linkErrorMessage ? (
            <div className="notice-block notice-block-error">
              <p>{linkErrorMessage}</p>
            </div>
          ) : null}
          <div className="actions">
            <button type="button" disabled={!canLink} onClick={onLink}>
              {isLinking ? "Linking..." : "Link planned session"}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

interface PlannedSessionComparisonBlockProps {
  details: WorkoutDetailsDto;
}

function PlannedSessionComparisonBlock({
  details,
}: PlannedSessionComparisonBlockProps) {
  const plannedSession = details.plannedSession;
  const plannedVsActual = details.plannedVsActual;

  if (plannedSession === null) {
    return null;
  }

  return (
    <div className="details-block">
      <h4>Planned session</h4>

      <div className="metric-table-wrapper">
        <table className="metric-table">
          <thead>
            <tr>
              <th>Metric</th>
              <th>Planned</th>
              <th>Actual</th>
              <th>Result</th>
            </tr>
          </thead>

          <tbody>
            <tr>
              <td>Type</td>
              <td>{plannedSession.type}</td>
              <td>{details.summary.type}</td>
              <td>
                {plannedVsActual?.type.isMatched ? "Matched" : "Different"}
              </td>
            </tr>

            <tr>
              <td>Intensity / RPE</td>
              <td>{plannedSession.intensity}</td>
              <td>RPE {details.summary.rpe}</td>
              <td>
                {plannedVsActual?.intensity.isMatched
                  ? "Within expected effort"
                  : "Different effort"}
              </td>
            </tr>

            <tr>
              <td>Duration</td>
              <td>{formatDuration(plannedSession.targetDurationSeconds)}</td>
              <td>{formatDuration(details.summary.durationSeconds)}</td>
              <td>{formatMetricResult(plannedVsActual?.duration.status)}</td>
            </tr>

            <tr>
              <td>Distance</td>
              <td>
                {plannedSession.targetDistanceKm === null
                  ? "-"
                  : `${plannedSession.targetDistanceKm} km`}
              </td>
              <td>{details.summary.distanceKm} km</td>
              <td>{formatMetricResult(plannedVsActual?.distance?.status)}</td>
            </tr>

            <tr>
              <td>Pace</td>
              <td>
                {plannedSession.targetPaceSecondsPerKm === null
                  ? "-"
                  : formatPace(plannedSession.targetPaceSecondsPerKm)}
              </td>
              <td>{formatPace(details.summary.averagePaceSecondsPerKm)}</td>
              <td>{formatMetricResult(plannedVsActual?.pace?.status)}</td>
            </tr>
          </tbody>
        </table>
      </div>

      {plannedVsActual && (
        <p className="muted comparison-summary">
          {formatComparisonSummary(plannedVsActual.status)}
        </p>
      )}
    </div>
  );
}

interface SplitsBlockProps {
  splits: WorkoutSplitDto[];
}

function SplitsBlock({ splits }: SplitsBlockProps) {
  if (splits.length === 0) {
    return null;
  }

  return (
    <div className="details-block">
      <h4>Splits</h4>

      <div className="metric-table-wrapper">
        <table className="metric-table">
          <thead>
            <tr>
              <th>Split</th>
              <th>Distance</th>
              <th>Duration</th>
              <th>Average pace</th>
            </tr>
          </thead>

          <tbody>
            {splits.map((split) => (
              <tr key={split.id}>
                <td>{split.splitNumber}</td>
                <td>{split.distanceKm} km</td>
                <td>{formatDuration(split.durationSeconds)}</td>
                <td>{formatPace(split.averagePaceSecondsPerKm)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

interface InsightsBlockProps {
  insights: WorkoutInsightDto[];
}

function InsightsBlock({ insights }: InsightsBlockProps) {
  const visibleInsights = insights.filter(isVisibleInsight);

  if (visibleInsights.length === 0) {
    return null;
  }

  return (
    <div className="details-block">
      <h4>Insights</h4>

      <div className="insights-list">
        {visibleInsights.map((insight) => (
          <article
            key={`${insight.title}-${insight.message}`}
            className={`insight-card insight-card-${insight.severity.toLowerCase()}`}
          >
            <strong>{insight.title}</strong>
            <p>{insight.message}</p>
          </article>
        ))}
      </div>
    </div>
  );
}

function formatPlannedSessionOption(session: PlannedSessionDto): string {
  const scheduledFor = session.scheduledFor
    ? formatDateTime(session.scheduledFor)
    : "Unscheduled";

  const distance =
    session.targetDistanceKm === null ? "-" : `${session.targetDistanceKm} km`;

  const pace =
    session.targetPaceSecondsPerKm === null
      ? "-"
      : formatPace(session.targetPaceSecondsPerKm);

  return `${session.status} · ${scheduledFor} · ${session.type} · ${session.intensity} · ${distance} · ${formatDuration(
    session.targetDurationSeconds,
  )} · ${pace}`;
}

function isVisibleInsight(insight: WorkoutInsightDto): boolean {
  return (
    insight.title === "Fastest split" ||
    insight.title === "Slowest split" ||
    insight.title === "Variable pacing"
  );
}

function formatMetricResult(status: string | undefined): string {
  switch (status) {
    case "Matched":
      return "Matched";

    case "LowerThanPlanned":
      return "Lower than planned";

    case "HigherThanPlanned":
      return "Higher than planned";

    case "FasterThanPlanned":
      return "Faster than planned";

    case "SlowerThanPlanned":
      return "Slower than planned";

    case "NotAvailable":
      return "Not available";

    default:
      return "-";
  }
}

function formatComparisonSummary(status: string): string {
  switch (status) {
    case "Matched":
      return "Actual workout matches the planned session.";

    case "PartiallyMatched":
      return "Actual workout partially matches the planned session.";

    case "NotMatched":
      return "Actual workout differs from the planned session.";

    default:
      return "Comparison is not available.";
  }
}

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Request failed.";
}