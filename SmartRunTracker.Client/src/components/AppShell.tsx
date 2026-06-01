import type { ReactNode } from "react";

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
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <h1 className="brand">Smart Run Tracker</h1>
          <p className="brand-subtitle">Adaptive running planner</p>
        </div>

        <nav className="nav">
          {navigationItems.map((item) => (
            <button
              key={item.page}
              type="button"
              className={
                activePage === item.page ? "nav-button active" : "nav-button"
              }
              onClick={() => onPageChange(item.page)}
            >
              {item.label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="main-content">{children}</main>
    </div>
  );
}