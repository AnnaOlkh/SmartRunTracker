import type {
  WorkoutDetailsDto,
  WorkoutInsightDto,
  WorkoutSplitDto,
} from "../api/types";
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
                {plannedVsActual?.type.isMatched
                  ? "Matched"
                  : "Different"}
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
      return "Within tolerance";

    case "LowerThanPlanned":
      return "Below planned";

    case "HigherThanPlanned":
      return "Above planned";

    case "FasterThanPlanned":
      return "Faster than planned";

    case "SlowerThanPlanned":
      return "Slower than planned";

    case "NotAvailable":
    case undefined:
      return "-";

    default:
      return String(status);
  }
}

function formatComparisonSummary(status: string): string {
  switch (status) {
    case "Matched":
      return "Completed according to plan.";

    case "PartiallyMatched":
      return "Partially completed according to plan.";

    case "NotMatched":
      return "Did not match the planned session.";

    default:
      return "Compared with planned session.";
  }
}