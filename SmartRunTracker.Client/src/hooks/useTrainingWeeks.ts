import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../api/queryKeys";
import { trainingWeeksApi } from "../api/trainingWeeksApi";
import type { GenerateTrainingWeekRequest } from "../api/types";

export function useTrainingWeeks() {
  return useQuery({
    queryKey: queryKeys.trainingWeeks,
    queryFn: trainingWeeksApi.getAll,
  });
}

export function useTrainingWeek(id: number | null) {
  return useQuery({
    queryKey: id === null ? ["training-weeks", "empty"] : queryKeys.trainingWeek(id),
    queryFn: () => {
      if (id === null) {
        throw new Error("Training week id is required.");
      }

      return trainingWeeksApi.getById(id);
    },
    enabled: id !== null,
  });
}

export function useGenerateTrainingWeek() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: GenerateTrainingWeekRequest) => {
      return trainingWeeksApi.generate(request);
    },
    onSuccess: async (week) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.trainingWeek(week.id),
        }),
      ]);
    },
  });
}