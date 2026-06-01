import { useMutation, useQueryClient } from "@tanstack/react-query";
import { plannedSessionsApi } from "../api/plannedSessionsApi";
import { queryKeys } from "../api/queryKeys";
import type { SchedulePlannedSessionRequest } from "../api/types";

export function useSchedulePlannedSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (variables: {
      id: number;
      request: SchedulePlannedSessionRequest;
    }) => {
      return plannedSessionsApi.schedule(variables.id, variables.request);
    },
    onSuccess: async (session) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.trainingWeek(session.trainingWeekId),
        }),
      ]);
    },
  });
}

export function useSkipPlannedSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => {
      return plannedSessionsApi.skip(id);
    },
    onSuccess: async (session) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.trainingWeek(session.trainingWeekId),
        }),
      ]);
    },
  });
}