import {
    useBootstrapDemoUser,
    useSeedDemoScenario,
    type DemoScenarioName,
  } from "../hooks/useDemo";
  
  const scenarios: Array<{
    value: DemoScenarioName;
    label: string;
    description: string;
  }> = [
    {
      value: "maintain",
      label: "Maintain",
      description: "Normal recent week without strong progress or deload signal.",
    },
    {
      value: "progress",
      label: "Progress",
      description: "Two stable weeks with controlled RPE and stable volume.",
    },
    {
      value: "deload",
      label: "Deload",
      description: "High RPE and too many quality workouts.",
    },
    {
      value: "skipped-sessions",
      label: "Skipped sessions",
      description: "Several planned sessions skipped last week.",
    },
    {
      value: "low-consistency",
      label: "Low consistency",
      description: "Previous normal week, but only one workout last week.",
    },
    {
      value: "six-days",
      label: "Six days",
      description: "Profile with six preferred workouts and six available days.",
    },
  ];
  
  export function DemoPage() {
    const bootstrapDemoUser = useBootstrapDemoUser();
    const seedDemoScenario = useSeedDemoScenario();
  
    return (
      <div className="page">
        <div className="page-header">
          <div>
            <h2>Demo scenarios</h2>
            <p>Seed backend data to test generator modes.</p>
          </div>
        </div>
  
        <section className="card">
          <h3>Bootstrap</h3>
          <p className="muted">
            Creates the demo user used by the MVP instead of full authentication.
          </p>
  
          <button
            type="button"
            onClick={() => bootstrapDemoUser.mutate()}
            disabled={bootstrapDemoUser.isPending}
          >
            Bootstrap demo user
          </button>
  
          {bootstrapDemoUser.data && (
            <p className="success">{bootstrapDemoUser.data.message}</p>
          )}
  
          {bootstrapDemoUser.error && (
            <p className="error">{bootstrapDemoUser.error.message}</p>
          )}
        </section>
  
        <section className="card">
          <h3>Scenario seeds</h3>
  
          <div className="scenario-grid">
            {scenarios.map((scenario) => (
              <article key={scenario.value} className="scenario-card">
                <h4>{scenario.label}</h4>
                <p className="muted">{scenario.description}</p>
  
                <button
                  type="button"
                  onClick={() => seedDemoScenario.mutate(scenario.value)}
                  disabled={seedDemoScenario.isPending}
                >
                  Seed {scenario.label}
                </button>
              </article>
            ))}
          </div>
  
          {seedDemoScenario.data && (
            <p className="success">{seedDemoScenario.data.message}</p>
          )}
  
          {seedDemoScenario.error && (
            <p className="error">{seedDemoScenario.error.message}</p>
          )}
        </section>
      </div>
    );
  }