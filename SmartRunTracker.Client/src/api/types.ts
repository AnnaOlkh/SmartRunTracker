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