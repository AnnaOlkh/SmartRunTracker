import { apiRequest } from "./client";
import type {
  CreateWorkoutRequest,
  ImportGpxWorkoutRequest,
  LinkWorkoutPlannedSessionRequest,
  PlannedSessionDto,
  UpdateWorkoutRequest,
  WorkoutDetailsDto,
  WorkoutDto,
} from "./types";

export const workoutsApi = {
  getAll: () => {
    return apiRequest<WorkoutDto[]>("/workouts");
  },

  getById: (id: number) => {
    return apiRequest<WorkoutDto>(`/workouts/${id}`);
  },

  getDetails: (id: number) => {
    return apiRequest<WorkoutDetailsDto>(`/workouts/${id}/details`);
  },
  getAvailablePlannedSessions: (id: number) => {
    return apiRequest<PlannedSessionDto[]>(
      `/workouts/${id}/available-planned-sessions`,
    );
  },
  
  linkPlannedSession: (
    id: number,
    request: LinkWorkoutPlannedSessionRequest,
  ) => {
    return apiRequest<WorkoutDetailsDto>(`/workouts/${id}/planned-session`, {
      method: "PATCH",
      body: request,
    });
  },
  create: (request: CreateWorkoutRequest) => {
    return apiRequest<WorkoutDto>("/workouts", {
      method: "POST",
      body: request,
    });
  },
  importGpx: (request: ImportGpxWorkoutRequest) => {
    const formData = new FormData();

    formData.append("file", request.file);

    if (request.plannedSessionId !== null) {
      formData.append("plannedSessionId", String(request.plannedSessionId));
    }

    if (request.workoutType !== null) {
      formData.append("workoutType", request.workoutType);
    }

    if (request.rpe !== null) {
      formData.append("rpe", String(request.rpe));
    }

    if (request.notes !== null && request.notes.trim() !== "") {
      formData.append("notes", request.notes.trim());
    }

    return apiRequest<WorkoutDto>("/workout-imports/gpx", {
      method: "POST",
      body: formData,
    });
  },

  update: (id: number, request: UpdateWorkoutRequest) => {
    return apiRequest<WorkoutDto>(`/workouts/${id}`, {
      method: "PUT",
      body: request,
    });
  },

  delete: (id: number) => {
    return apiRequest<void>(`/workouts/${id}`, {
      method: "DELETE",
    });
  },
};