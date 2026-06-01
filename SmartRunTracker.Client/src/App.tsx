import { useState } from "react";
import { AppShell, type AppPage } from "./components/AppShell";
import { DashboardPage } from "./pages/DashboardPage";
import { DemoPage } from "./pages/DemoPage";
import { GoalPage } from "./pages/GoalPage";
import { ProfilePage } from "./pages/ProfilePage";
import { TrainingPlanPage } from "./pages/TrainingPlanPage";
import { WorkoutsPage } from "./pages/WorkoutsPage";

export default function App() {
  const [activePage, setActivePage] = useState<AppPage>("dashboard");

  return (
    <AppShell activePage={activePage} onPageChange={setActivePage}>
      {activePage === "dashboard" && <DashboardPage />}
      {activePage === "demo" && <DemoPage />}
      {activePage === "training-plan" && <TrainingPlanPage />}
      {activePage === "workouts" && <WorkoutsPage />}
      {activePage === "goal" && <GoalPage />}
      {activePage === "profile" && <ProfilePage />}
    </AppShell>
  );
}