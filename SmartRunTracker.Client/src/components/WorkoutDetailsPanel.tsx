import type {
    PlannedVsActualDto,
    PlannedVsActualMetricDto,
    WorkoutDetailsDto,
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
      <section className="card">
        <div className="list-item-header">
          <div>
            <h3>Workout details</h3>
            <p className="muted">
              Workout #{summary.id} · {summary.source} ·{" "}
              {formatDateTime(summary.startedAt)}
            </p>
          </div>
  
          <button type="button" className="secondary-button" onClick={onClose}>
            Close
          </button>
        </div>
  
        <div className="stats-grid">
          <div className="stat-card">
            <span>Distance</span>
            <strong>{summary.distanceKm} km</strong>
          </div>
  
          <div className="stat-card">
            <span>Duration</span>
            <strong>{formatDuration(summary.durationSeconds)}</strong>
          </div>
  
          <div className="stat-card">
            <span>Average pace</span>
            <strong>{formatPace(summary.averagePaceSecondsPerKm)}</strong>
          </div>
  
          <div className="stat-card">
            <span>RPE</span>
            <strong>{summary.rpe}</strong>
          </div>
  
          <div className="stat-card">
            <span>Load</span>
            <strong>{summary.sessionLoad}</strong>
          </div>
  
          <div className="stat-card">
            <span>Type</span>
            <strong>{summary.type}</strong>
          </div>
        </div>
  
        {summary.notes && (
          <div className="details-block">
            <h4>Notes</h4>
            <p>{summary.notes}</p>
          </div>
        )}
  
        <div className="details-block">
            <h4>Route</h4>

            {details.hasRouteData ? (
            <>
                <p className="success">
                Route data is available. Points: {details.routePoints.length}.
                </p>

                <WorkoutRouteMap routePoints={details.routePoints} />
            </>
            ) : (
            <p className="muted">
                This workout has no route data. Summary and planned comparison are still
                available.
            </p>
            )}
        </div>
  
        <PlannedSessionBlock details={details} />
  
        <PlannedVsActualBlock plannedVsActual={details.plannedVsActual} />
  
        <SplitsBlock splits={details.splits} />
      </section>
    );
  }
  
  interface PlannedSessionBlockProps {
    details: WorkoutDetailsDto;
  }
  
  function PlannedSessionBlock({ details }: PlannedSessionBlockProps) {
    const plannedSession = details.plannedSession;
  
    if (plannedSession === null) {
      return (
        <div className="details-block">
          <h4>Planned session</h4>
          <p className="muted">This workout is not linked to a planned session.</p>
        </div>
      );
    }
  
    return (
      <div className="details-block">
        <h4>Planned session</h4>
  
        <div className="stats-grid">
          <div className="stat-card">
            <span>Status</span>
            <strong>{plannedSession.status}</strong>
          </div>
  
          <div className="stat-card">
            <span>Type</span>
            <strong>{plannedSession.type}</strong>
          </div>
  
          <div className="stat-card">
            <span>Intensity</span>
            <strong>{plannedSession.intensity}</strong>
          </div>
  
          <div className="stat-card">
            <span>Target duration</span>
            <strong>{formatDuration(plannedSession.targetDurationSeconds)}</strong>
          </div>
  
          <div className="stat-card">
            <span>Target distance</span>
            <strong>
              {plannedSession.targetDistanceKm === null
                ? "-"
                : `${plannedSession.targetDistanceKm} km`}
            </strong>
          </div>
  
          <div className="stat-card">
            <span>Target pace</span>
            <strong>
              {plannedSession.targetPaceSecondsPerKm === null
                ? "-"
                : formatPace(plannedSession.targetPaceSecondsPerKm)}
            </strong>
          </div>
        </div>
      </div>
    );
  }
  
  interface PlannedVsActualBlockProps {
    plannedVsActual: PlannedVsActualDto | null;
  }
  
  function PlannedVsActualBlock({
    plannedVsActual,
  }: PlannedVsActualBlockProps) {
    if (plannedVsActual === null) {
      return (
        <div className="details-block">
          <h4>Planned vs actual</h4>
          <p className="muted">
            No planned-vs-actual comparison is available because this workout is
            not linked to a planned session.
          </p>
        </div>
      );
    }
  
    return (
      <div className="details-block">
        <h4>Planned vs actual</h4>
  
        <p>
          <strong>Status:</strong> {plannedVsActual.status}
        </p>
  
        <ul className="compact-list">
          {plannedVsActual.messages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
  
        <div className="metric-table-wrapper">
          <table className="metric-table">
            <thead>
              <tr>
                <th>Metric</th>
                <th>Planned</th>
                <th>Actual</th>
                <th>Difference</th>
                <th>Status</th>
              </tr>
            </thead>
  
            <tbody>
              <MetricRow metric={plannedVsActual.duration} />
  
              {plannedVsActual.distance && (
                <MetricRow metric={plannedVsActual.distance} />
              )}
  
              {plannedVsActual.pace && <MetricRow metric={plannedVsActual.pace} />}
  
              <tr>
                <td>Type</td>
                <td>{plannedVsActual.type.plannedType}</td>
                <td>{plannedVsActual.type.actualType}</td>
                <td>-</td>
                <td>{plannedVsActual.type.isMatched ? "Matched" : "Different"}</td>
              </tr>
  
              <tr>
                <td>Intensity</td>
                <td>{plannedVsActual.intensity.plannedIntensity}</td>
                <td>RPE {plannedVsActual.intensity.actualRpe}</td>
                <td>-</td>
                <td>
                  {plannedVsActual.intensity.isMatched
                    ? "Matched"
                    : "Different"}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    );
  }
  
  interface MetricRowProps {
    metric: PlannedVsActualMetricDto;
  }
  
  function MetricRow({ metric }: MetricRowProps) {
    return (
      <tr>
        <td>{metric.name}</td>
        <td>{formatMetricValue(metric.plannedValue, metric.unit)}</td>
        <td>{formatMetricValue(metric.actualValue, metric.unit)}</td>
        <td>
          {metric.difference === null
            ? "-"
            : `${formatMetricValue(metric.difference, metric.unit)}${
                metric.differencePercent === null
                  ? ""
                  : ` (${metric.differencePercent}%)`
              }`}
        </td>
        <td>{metric.status}</td>
      </tr>
    );
  }
  
  interface SplitsBlockProps {
    splits: WorkoutSplitDto[];
  }
  
  function SplitsBlock({ splits }: SplitsBlockProps) {
    if (splits.length === 0) {
      return (
        <div className="details-block">
          <h4>Splits</h4>
          <p className="muted">No split data is available for this workout.</p>
        </div>
      );
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
  
  function formatMetricValue(value: number | null, unit: string): string {
    if (value === null) {
      return "-";
    }
  
    if (unit === "seconds") {
      return formatDuration(value);
    }
  
    if (unit === "seconds_per_km") {
      return formatPace(value);
    }
  
    if (unit === "km") {
      return `${Number(value.toFixed(2))} km`;
    }
  
    return String(value);
  }