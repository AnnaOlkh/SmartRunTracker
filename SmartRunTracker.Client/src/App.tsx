import { useState } from "react";
import { useAuth } from "./auth/useAuth";
import { AppShell, type AppPage } from "./components/AppShell";
import { DashboardPage } from "./pages/DashboardPage";
import { DemoPage } from "./pages/DemoPage";
import { GoalPage } from "./pages/GoalPage";
import { LoginPage } from "./pages/LoginPage";
import { ProfilePage } from "./pages/ProfilePage";
import { RegisterPage } from "./pages/RegisterPage";
import { TrainingPlanPage } from "./pages/TrainingPlanPage";
import { WorkoutsPage } from "./pages/WorkoutsPage";

type AuthPage = "login" | "register";

export default function App() {
  const { isAuthenticated, isLoading } = useAuth();

  const [activePage, setActivePage] = useState<AppPage>("dashboard");
  const [authPage, setAuthPage] = useState<AuthPage>("login");

  if (isLoading) {
    return (
      <main className="auth-page">
        <section className="auth-card">
          <p className="eyebrow">Smart Run Tracker</p>
          <h1>Loading...</h1>
        </section>
      </main>
    );
  }

  if (!isAuthenticated) {
    if (authPage === "register") {
      return <RegisterPage onSwitchToLogin={() => setAuthPage("login")} />;
    }

    return <LoginPage onSwitchToRegister={() => setAuthPage("register")} />;
  }

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