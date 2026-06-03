import { useEffect, useMemo } from "react";
import {
  CircleMarker,
  MapContainer,
  Polyline,
  TileLayer,
  useMap,
} from "react-leaflet";
import type { LatLngBoundsExpression, LatLngExpression } from "leaflet";
import type { WorkoutRoutePointDto } from "../api/types";
import { formatPace } from "../utils/format";

interface WorkoutRouteMapProps {
  routePoints: WorkoutRoutePointDto[];
}

interface RouteSegment {
  id: string;
  positions: LatLngExpression[];
  paceSecondsPerKm: number | null;
  paceLevel: PaceLevel;
}

type PaceLevel = "fast" | "steady" | "slow" | "unknown";

export function WorkoutRouteMap({ routePoints }: WorkoutRouteMapProps) {
  const orderedPoints = useMemo(() => {
    return routePoints.slice().sort((a, b) => a.order - b.order);
  }, [routePoints]);

  const positions = useMemo<LatLngExpression[]>(() => {
    return orderedPoints.map((point) => [point.latitude, point.longitude]);
  }, [orderedPoints]);

  const segments = useMemo(() => {
    return buildRouteSegments(orderedPoints);
  }, [orderedPoints]);

  const startPosition = positions[0];
  const endPosition = positions[positions.length - 1];

  if (positions.length < 2) {
    return (
      <div className="route-map-empty">
        Not enough route points to display a map.
      </div>
    );
  }

  return (
    <div className="route-map-layout">
      <div className="route-map">
        <MapContainer
          center={startPosition}
          zoom={14}
          scrollWheelZoom={false}
          className="route-map-container"
        >
          <TileLayer
            attribution="&copy; OpenStreetMap contributors"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          <FitRouteBounds positions={positions} />

          {segments.map((segment) => (
            <Polyline
              key={segment.id}
              positions={segment.positions}
              pathOptions={{
                color: getPaceColor(segment.paceLevel),
                weight: 5,
                opacity: 0.9,
              }}
            />
          ))}

          <CircleMarker
            center={startPosition}
            radius={7}
            pathOptions={{
              color: "#166534",
              fillColor: "#22c55e",
              fillOpacity: 1,
            }}
          >
            <title>Start</title>
          </CircleMarker>

          <CircleMarker
            center={endPosition}
            radius={7}
            pathOptions={{
              color: "#991b1b",
              fillColor: "#ef4444",
              fillOpacity: 1,
            }}
          >
            <title>Finish</title>
          </CircleMarker>
        </MapContainer>
      </div>

      <RoutePaceLegend segments={segments} />
    </div>
  );
}

interface FitRouteBoundsProps {
  positions: LatLngExpression[];
}

function FitRouteBounds({ positions }: FitRouteBoundsProps) {
  const map = useMap();

  useEffect(() => {
    if (positions.length < 2) {
      return;
    }

    const bounds = positions as LatLngBoundsExpression;

    map.fitBounds(bounds, {
      padding: [24, 24],
    });
  }, [map, positions]);

  return null;
}

interface RoutePaceLegendProps {
  segments: RouteSegment[];
}

function RoutePaceLegend({ segments }: RoutePaceLegendProps) {
  const paces = segments
    .map((segment) => segment.paceSecondsPerKm)
    .filter((pace): pace is number => pace !== null);

  if (paces.length === 0) {
    return (
      <div className="route-pace-legend">
        <h5>Route pace</h5>
        <p className="muted">No segment pace data is available.</p>
      </div>
    );
  }

  const fastest = Math.min(...paces);
  const slowest = Math.max(...paces);
  const average = Math.round(
    paces.reduce((sum, pace) => sum + pace, 0) / paces.length,
  );

  return (
    <div className="route-pace-legend">
      <h5>Route pace</h5>

      <div className="route-pace-summary">
        <span>Fastest segment: {formatPace(fastest)}</span>
        <span>Average segment: {formatPace(average)}</span>
        <span>Slowest segment: {formatPace(slowest)}</span>
      </div>

      <div className="route-pace-scale">
        <span>
          <span className="pace-dot pace-dot-fast" />
          Faster
        </span>

        <span>
          <span className="pace-dot pace-dot-steady" />
          Near average
        </span>

        <span>
          <span className="pace-dot pace-dot-slow" />
          Slower
        </span>

        <span>
          <span className="pace-dot pace-dot-unknown" />
          Unknown
        </span>
      </div>
    </div>
  );
}

function buildRouteSegments(points: WorkoutRoutePointDto[]): RouteSegment[] {
  if (points.length < 2) {
    return [];
  }

  const paces = points
    .map((point) => point.paceSecondsPerKm)
    .filter((pace): pace is number => pace !== null && pace > 0);

  const averagePace =
    paces.length === 0
      ? null
      : paces.reduce((sum, pace) => sum + pace, 0) / paces.length;

  const segments: RouteSegment[] = [];

  for (let index = 1; index < points.length; index += 1) {
    const previous = points[index - 1];
    const current = points[index];

    const paceSecondsPerKm = current.paceSecondsPerKm;

    segments.push({
      id: `${previous.order}-${current.order}`,
      positions: [
        [previous.latitude, previous.longitude],
        [current.latitude, current.longitude],
      ],
      paceSecondsPerKm,
      paceLevel: getPaceLevel(paceSecondsPerKm, averagePace),
    });
  }

  return segments;
}

function getPaceLevel(
  paceSecondsPerKm: number | null,
  averagePaceSecondsPerKm: number | null,
): PaceLevel {
  if (
    paceSecondsPerKm === null ||
    paceSecondsPerKm <= 0 ||
    averagePaceSecondsPerKm === null ||
    averagePaceSecondsPerKm <= 0
  ) {
    return "unknown";
  }

  if (paceSecondsPerKm <= averagePaceSecondsPerKm * 0.92) {
    return "fast";
  }

  if (paceSecondsPerKm >= averagePaceSecondsPerKm * 1.08) {
    return "slow";
  }

  return "steady";
}

function getPaceColor(level: PaceLevel): string {
  switch (level) {
    case "fast":
      return "#16a34a";

    case "steady":
      return "#2563eb";

    case "slow":
      return "#dc2626";

    case "unknown":
      return "#6b7280";

    default:
      return "#6b7280";
  }
}