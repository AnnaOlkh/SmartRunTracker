import { useState } from "react";
import type { FormEvent } from "react";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/useAuth";

interface RegisterPageProps {
  onSwitchToLogin: () => void;
}

export function RegisterPage({ onSwitchToLogin }: RegisterPageProps) {
  const { register } = useAuth();

  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setError(null);
    setIsSubmitting(true);

    try {
      await register({ displayName, email, password });
    } catch (exception) {
      if (exception instanceof ApiError) {
        setError(exception.message);
      } else {
        setError("Registration failed.");
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-card">
        <p className="eyebrow">Smart Run Tracker</p>
        <h1>Create account</h1>
        <p className="muted">
          Create an account to keep your profile, running goals and workouts
          isolated from other users.
        </p>

        <form className="auth-form-grid" onSubmit={handleSubmit}>
          <label>
            Display name
            <input
              value={displayName}
              autoComplete="name"
              onChange={(event) => setDisplayName(event.target.value)}
              required
            />
          </label>

          <label>
            Email
            <input
              type="email"
              value={email}
              autoComplete="email"
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              autoComplete="new-password"
              minLength={6}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>

          {error && <div className="auth-error-message">{error}</div>}

          <button className="auth-primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating account..." : "Create account"}
          </button>
        </form>

        <button className="auth-link-button" type="button" onClick={onSwitchToLogin}>
          Already have an account
        </button>
      </section>
    </main>
  );
}