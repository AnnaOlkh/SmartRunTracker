import type { ReactNode } from "react";
import { useAuth } from "../auth/useAuth";

export type AppPage =
  | "dashboard"
  | "workouts"
  | "goal"
  | "profile"
  | "training-plan"
  | "demo";

interface AppShellProps {
  activePage: AppPage;
  onPageChange: (page: AppPage) => void;
  children: ReactNode;
}

const navigationItems: Array<{
  page: AppPage;
  label: string;
}> = [
  { page: "dashboard", label: "Dashboard" },
  { page: "workouts", label: "Workouts" },
  { page: "goal", label: "Goal" },
  { page: "profile", label: "Profile" },
  { page: "training-plan", label: "Training Plan" },
  { page: "demo", label: "Demo" },
];

export function AppShell({
  activePage,
  onPageChange,
  children,
}: AppShellProps) {
  const { user, logout } = useAuth();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <h1 className="brand">
          Smart
          <br />
          Run
          <br />
          Tracker
        </h1>

        <p className="brand-subtitle">Adaptive running planner</p>

        {user && (
          <div className="sidebar-user">
            <strong>{user.displayName}</strong>
            <span>{user.email}</span>
          </div>
        )}

        <nav className="nav">
          {navigationItems.map((item) => (
            <button
              key={item.page}
              type="button"
              className={`nav-button ${item.page === activePage ? "active" : ""}`}
              onClick={() => onPageChange(item.page)}
            >
              {item.label}
            </button>
          ))}
        </nav>

        <button
          className="secondary-button sidebar-logout"
          type="button"
          onClick={() => void logout()}
        >
          Logout
        </button>
      </aside>

      <main className="main-content">{children}</main>
    </div>
  );
}