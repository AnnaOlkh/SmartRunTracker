export const queryKeys = {
  workouts: ["workouts"] as const,
  workoutDetails: (id: number) => ["workouts", id, "details"] as const,

  runnerProfile: ["runner-profile"] as const,

  runningGoals: ["running-goals"] as const,
  activeRunningGoal: ["running-goals", "active"] as const,

  trainingWeeks: ["training-weeks"] as const,
  trainingWeek: (id: number) => ["training-weeks", id] as const,
};