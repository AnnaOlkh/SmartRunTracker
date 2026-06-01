import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../api/queryKeys";
import { workoutsApi } from "../api/workoutsApi";
import type { CreateWorkoutRequest, UpdateWorkoutRequest } from "../api/types";

export function useWorkouts() {
  return useQuery({
    queryKey: queryKeys.workouts,
    queryFn: workoutsApi.getAll,
  });
}

export function useCreateWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateWorkoutRequest) => {
      return workoutsApi.create(request);
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.workouts }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
      ]);
    },
  });
}

export function useUpdateWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (variables: {
      id: number;
      request: UpdateWorkoutRequest;
    }) => {
      return workoutsApi.update(variables.id, variables.request);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.workouts });
    },
  });
}

export function useDeleteWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => {
      return workoutsApi.delete(id);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.workouts });
    },
  });
}