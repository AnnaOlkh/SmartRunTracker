import { apiRequest } from "./client";
import type {
  CreateWorkoutRequest,
  UpdateWorkoutRequest,
  WorkoutDto,
} from "./types";

export const workoutsApi = {
  getAll: () => {
    return apiRequest<WorkoutDto[]>("/workouts");
  },

  getById: (id: number) => {
    return apiRequest<WorkoutDto>(`/workouts/${id}`);
  },

  create: (request: CreateWorkoutRequest) => {
    return apiRequest<WorkoutDto>("/workouts", {
      method: "POST",
      body: request,
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