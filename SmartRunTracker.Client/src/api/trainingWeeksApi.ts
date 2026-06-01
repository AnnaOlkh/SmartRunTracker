import { apiRequest } from "./client";
import type { GenerateTrainingWeekRequest, TrainingWeekDto } from "./types";

export const trainingWeeksApi = {
  getAll: () => {
    return apiRequest<TrainingWeekDto[]>("/training-weeks");
  },

  getById: (id: number) => {
    return apiRequest<TrainingWeekDto>(`/training-weeks/${id}`);
  },

  generate: (request: GenerateTrainingWeekRequest) => {
    return apiRequest<TrainingWeekDto>("/training-weeks/generate", {
      method: "POST",
      body: request,
    });
  },
};