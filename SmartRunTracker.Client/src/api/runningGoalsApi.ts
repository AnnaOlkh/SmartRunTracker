import { apiRequest } from "./client";
import type { CreateRunningGoalRequest, RunningGoalDto } from "./types";

export const runningGoalsApi = {
  getAll: () => {
    return apiRequest<RunningGoalDto[]>("/running-goals");
  },

  getActive: () => {
    return apiRequest<RunningGoalDto>("/running-goals/active");
  },

  create: (request: CreateRunningGoalRequest) => {
    return apiRequest<RunningGoalDto>("/running-goals", {
      method: "POST",
      body: request,
    });
  },

  activate: (id: number) => {
    return apiRequest<RunningGoalDto>(`/running-goals/${id}/activate`, {
      method: "PATCH",
    });
  },

  delete: (id: number) => {
    return apiRequest<void>(`/running-goals/${id}`, {
      method: "DELETE",
    });
  },
};