import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "../api/queryKeys";
import { runnerProfileApi } from "../api/runnerProfileApi";
import type { UpdateRunnerProfileRequest } from "../api/types";

export function useRunnerProfile() {
  return useQuery({
    queryKey: queryKeys.runnerProfile,
    queryFn: runnerProfileApi.get,
  });
}

export function useUpdateRunnerProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateRunnerProfileRequest) => {
      return runnerProfileApi.update(request);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: queryKeys.runnerProfile,
      });
    },
  });
}