import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../api/queryKeys";
import { workoutsApi } from "../api/workoutsApi";
import type {
  CreateWorkoutRequest,
  ImportGpxWorkoutRequest,
  UpdateWorkoutRequest,
} from "../api/types";

export function useWorkouts() {
  return useQuery({
    queryKey: queryKeys.workouts,
    queryFn: workoutsApi.getAll,
  });
}

export function useWorkoutDetails(id: number | null) {
  return useQuery({
    queryKey: id === null ? ["workouts", "details", "empty"] : queryKeys.workoutDetails(id),
    queryFn: () => {
      if (id === null) {
        throw new Error("Workout id is required.");
      }

      return workoutsApi.getDetails(id);
    },
    enabled: id !== null,
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

export function useImportGpxWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ImportGpxWorkoutRequest) => {
      return workoutsApi.importGpx(request);
    },
    onSuccess: async (workout) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.workouts }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.workoutDetails(workout.id),
        }),
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
    onSuccess: async (_workout, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.workouts }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.workoutDetails(variables.id),
        }),
      ]);
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