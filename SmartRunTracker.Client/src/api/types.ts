export type WorkoutType = "Easy" | "Quality" | "Long" | "Recovery";

export type WorkoutSource = "Manual" | "Strava" | "Gpx" | "Tcx";

export type WorkoutIntensity = "Low" | "Moderate" | "High";

export type AdjustmentMode = "Deload" | "Maintain" | "Progress";

export type TrainingWeekStatus = "Draft" | "Active" | "Completed";

export type PlannedSessionStatus =
  | "Unscheduled"
  | "Scheduled"
  | "Completed"
  | "Skipped";

export type TrainingDayPreferenceMode = "AnyDay" | "SelectedDays";

export type TrainingDay =
  | "Monday"
  | "Tuesday"
  | "Wednesday"
  | "Thursday"
  | "Friday"
  | "Saturday"
  | "Sunday";

export interface WorkoutDto {
  id: number;
  userId: number;
  plannedSessionId: number | null;
  startedAt: string;
  distanceKm: number;
  durationSeconds: number;
  averagePaceSecondsPerKm: number;
  rpe: number;
  sessionLoad: number;
  type: WorkoutType;
  source: WorkoutSource;
  notes: string | null;
  createdAt: string;
}
export type PlannedVsActualStatus =
  | "Matched"
  | "PartiallyMatched"
  | "NotMatched";

export type PlannedVsActualMetricStatus =
  | "Matched"
  | "LowerThanPlanned"
  | "HigherThanPlanned"
  | "FasterThanPlanned"
  | "SlowerThanPlanned"
  | "NotAvailable";

export interface WorkoutRoutePointDto {
  id: number;
  order: number;
  latitude: number;
  longitude: number;
  elevationMeters: number | null;
  recordedAt: string | null;
  distanceFromStartMeters: number;
  secondsFromStart: number | null;
  paceSecondsPerKm: number | null;
}

export interface WorkoutSplitDto {
  id: number;
  splitNumber: number;
  distanceKm: number;
  durationSeconds: number;
  averagePaceSecondsPerKm: number;
  startedAt: string | null;
  endedAt: string | null;
}

export interface WorkoutPlannedSessionDto {
  id: number;
  type: WorkoutType;
  intensity: WorkoutIntensity;
  targetDurationSeconds: number;
  targetDistanceKm: number | null;
  targetPaceSecondsPerKm: number | null;
  scheduledFor: string | null;
  status: PlannedSessionStatus;
}

export interface PlannedVsActualMetricDto {
  name: string;
  plannedValue: number | null;
  actualValue: number;
  difference: number | null;
  differencePercent: number | null;
  unit: string;
  status: PlannedVsActualMetricStatus;
  message: string;
}

export interface PlannedVsActualTypeDto {
  plannedType: WorkoutType;
  actualType: WorkoutType;
  isMatched: boolean;
  message: string;
}

export interface PlannedVsActualIntensityDto {
  plannedIntensity: WorkoutIntensity;
  actualRpe: number;
  isMatched: boolean;
  message: string;
}

export interface PlannedVsActualDto {
  status: PlannedVsActualStatus;
  messages: string[];
  duration: PlannedVsActualMetricDto;
  distance: PlannedVsActualMetricDto | null;
  pace: PlannedVsActualMetricDto | null;
  type: PlannedVsActualTypeDto;
  intensity: PlannedVsActualIntensityDto;
}

export type WorkoutInsightSeverity = "Info" | "Positive" | "Warning";

export interface WorkoutInsightDto {
  title: string;
  message: string;
  severity: WorkoutInsightSeverity;
}

export interface WorkoutDetailsDto {
  summary: WorkoutDto;
  plannedSession: WorkoutPlannedSessionDto | null;
  plannedVsActual: PlannedVsActualDto | null;
  routePoints: WorkoutRoutePointDto[];
  splits: WorkoutSplitDto[];
  hasRouteData: boolean;
  hasSplits: boolean;
  insights: WorkoutInsightDto[];
}

export interface ImportGpxWorkoutRequest {
  file: File;
  plannedSessionId: number | null;
  workoutType: WorkoutType | null;
  rpe: number | null;
  notes: string | null;
}

export interface CreateWorkoutRequest {
  startedAt: string;
  distanceKm: number;
  durationSeconds: number;
  rpe: number;
  type: WorkoutType;
  notes: string | null;
  plannedSessionId: number | null;
}

export interface UpdateWorkoutRequest {
  startedAt: string;
  distanceKm: number;
  durationSeconds: number;
  rpe: number;
  type: WorkoutType;
  notes: string | null;
}

export interface RunnerProfileDto {
  id: number;
  userId: number;
  preferredWorkoutsPerWeek: number;
  trainingDayPreferenceMode: TrainingDayPreferenceMode;
  availableDays: TrainingDay[];
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateRunnerProfileRequest {
  preferredWorkoutsPerWeek: number;
  trainingDayPreferenceMode: TrainingDayPreferenceMode;
  availableDays: TrainingDay[];
}

export interface RunningGoalDto {
  id: number;
  userId: number;
  targetDistanceKm: number;
  targetPaceSecondsPerKm: number;
  targetFinishSeconds: number;
  goalDate: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateRunningGoalRequest {
  targetDistanceKm: number;
  targetPaceSecondsPerKm: number;
  goalDate: string | null;
}

export interface PlannedSessionDto {
  id: number;
  trainingWeekId: number;
  type: WorkoutType;
  intensity: WorkoutIntensity;
  targetDurationSeconds: number;
  targetDistanceKm: number | null;
  targetPaceSecondsPerKm: number | null;
  scheduledFor: string | null;
  status: PlannedSessionStatus;
  notes: string | null;
  reason: string | null;
  sortOrder: number;
}

export interface TrainingWeekDto {
  id: number;
  userId: number;
  runningGoalId: number;
  weekStartDate: string;
  targetWorkoutCount: number;
  adjustmentMode: AdjustmentMode;
  targetWeekDurationSeconds: number;
  status: TrainingWeekStatus;
  explanation: string | null;
  generatedAt: string;
  plannedSessions: PlannedSessionDto[];
}

export interface GenerateTrainingWeekRequest {
  weekStartDate: string;
}

export interface SchedulePlannedSessionRequest {
  scheduledFor: string;
}

export interface DemoBootstrapResponse {
  userId: number;
  displayName: string;
  message: string;
}

export interface DemoScenarioResultDto {
  scenario: string;
  weekStartDate: string;
  message: string;
}