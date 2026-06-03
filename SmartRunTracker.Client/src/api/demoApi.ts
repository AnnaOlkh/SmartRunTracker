import { apiRequest } from "./client";
import type { DemoBootstrapResponse, DemoScenarioResultDto } from "./types";

export const demoApi = {
  bootstrap: () => {
    return apiRequest<DemoBootstrapResponse>("/demo/bootstrap", {
      method: "POST",
    });
  },

  seedMaintainScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/maintain", {
      method: "POST",
    });
  },

  seedProgressScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/progress", {
      method: "POST",
    });
  },

  seedDeloadScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/deload", {
      method: "POST",
    });
  },

  seedSkippedSessionsScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/skipped-sessions", {
      method: "POST",
    });
  },

  seedLowConsistencyScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/low-consistency", {
      method: "POST",
    });
  },

  seedSixDaysScenario: () => {
    return apiRequest<DemoScenarioResultDto>("/demo-scenarios/six-days", {
      method: "POST",
    });
  },
};
export interface DemoSeedResult {
  userId: number;
  goalId: number;
  workoutCount: number;
  gpxWorkoutCount: number;
  from: string;
  to: string;
}

export function seedMay2026(): Promise<DemoSeedResult> {
  return apiRequest<DemoSeedResult>("/demo/seed-may-2026", {
    method: "POST",
  });
}