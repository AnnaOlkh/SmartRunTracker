import { apiRequest } from "./client";
import type { RunnerProfileDto, UpdateRunnerProfileRequest } from "./types";

export const runnerProfileApi = {
  get: () => {
    return apiRequest<RunnerProfileDto>("/runner-profile");
  },

  update: (request: UpdateRunnerProfileRequest) => {
    return apiRequest<RunnerProfileDto>("/runner-profile", {
      method: "PUT",
      body: request,
    });
  },
};