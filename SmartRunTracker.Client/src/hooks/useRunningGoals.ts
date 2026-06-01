import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../api/queryKeys";
import { runningGoalsApi } from "../api/runningGoalsApi";
import type { CreateRunningGoalRequest } from "../api/types";

export function useRunningGoals() {
  return useQuery({
    queryKey: queryKeys.runningGoals,
    queryFn: runningGoalsApi.getAll,
  });
}

export function useActiveRunningGoal() {
  return useQuery({
    queryKey: queryKeys.activeRunningGoal,
    queryFn: runningGoalsApi.getActive,
    retry: false,
  });
}

export function useCreateRunningGoal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateRunningGoalRequest) => {
      return runningGoalsApi.create(request);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.runningGoals }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.activeRunningGoal,
        }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
      ]);
    },
  });
}

export function useActivateRunningGoal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => {
      return runningGoalsApi.activate(id);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.runningGoals }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.activeRunningGoal,
        }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
      ]);
    },
  });
}

export function useDeleteRunningGoal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => {
      return runningGoalsApi.delete(id);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.runningGoals }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.activeRunningGoal,
        }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
      ]);
    },
  });
}