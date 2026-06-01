import { apiRequest } from "./client";
import type { PlannedSessionDto, SchedulePlannedSessionRequest } from "./types";

export const plannedSessionsApi = {
  schedule: (id: number, request: SchedulePlannedSessionRequest) => {
    return apiRequest<PlannedSessionDto>(`/planned-sessions/${id}/schedule`, {
      method: "PATCH",
      body: request,
    });
  },

  skip: (id: number) => {
    return apiRequest<PlannedSessionDto>(`/planned-sessions/${id}/skip`, {
      method: "PATCH",
    });
  },
};