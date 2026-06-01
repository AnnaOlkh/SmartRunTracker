import { useMutation, useQueryClient } from "@tanstack/react-query";
import { demoApi } from "../api/demoApi";
import { queryKeys } from "../api/queryKeys";

export type DemoScenarioName =
  | "maintain"
  | "progress"
  | "deload"
  | "skipped-sessions"
  | "low-consistency"
  | "six-days";

export function useBootstrapDemoUser() {
  return useMutation({
    mutationFn: demoApi.bootstrap,
  });
}

export function useSeedDemoScenario() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (scenario: DemoScenarioName) => {
      switch (scenario) {
        case "maintain":
          return demoApi.seedMaintainScenario();

        case "progress":
          return demoApi.seedProgressScenario();

        case "deload":
          return demoApi.seedDeloadScenario();

        case "skipped-sessions":
          return demoApi.seedSkippedSessionsScenario();

        case "low-consistency":
          return demoApi.seedLowConsistencyScenario();

        case "six-days":
          return demoApi.seedSixDaysScenario();

        default:
          throw new Error("Unknown demo scenario.");
      }
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.workouts }),
        queryClient.invalidateQueries({ queryKey: queryKeys.runnerProfile }),
        queryClient.invalidateQueries({ queryKey: queryKeys.runningGoals }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.activeRunningGoal,
        }),
        queryClient.invalidateQueries({ queryKey: queryKeys.trainingWeeks }),
      ]);
    },
  });
}