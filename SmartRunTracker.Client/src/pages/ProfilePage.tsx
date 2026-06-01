import { useState } from "react";
import type {
  RunnerProfileDto,
  TrainingDay,
  TrainingDayPreferenceMode,
} from "../api/types";
import {
  useRunnerProfile,
  useUpdateRunnerProfile,
} from "../hooks/useRunnerProfile";

const trainingDays: TrainingDay[] = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

export function ProfilePage() {
  const profile = useRunnerProfile();

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h2>Runner Profile</h2>
          <p>Configure training frequency and available training days.</p>
        </div>
      </div>

      {profile.isLoading && (
        <section className="card">
          <p className="muted">Loading profile...</p>
        </section>
      )}

      {profile.error && (
        <section className="card">
          <p className="error">{profile.error.message}</p>
        </section>
      )}

      {profile.data && (
        <ProfileForm
          key={`${profile.data.id}-${profile.data.updatedAt ?? profile.data.createdAt}`}
          profile={profile.data}
        />
      )}
    </div>
  );
}

function ProfileForm({ profile }: { profile: RunnerProfileDto }) {
  const updateProfile = useUpdateRunnerProfile();

  const [preferredWorkoutsPerWeek, setPreferredWorkoutsPerWeek] = useState(
    String(profile.preferredWorkoutsPerWeek),
  );

  const [trainingDayPreferenceMode, setTrainingDayPreferenceMode] =
    useState<TrainingDayPreferenceMode>(profile.trainingDayPreferenceMode);

  const [availableDays, setAvailableDays] = useState<TrainingDay[]>(
    profile.availableDays,
  );

  function toggleDay(day: TrainingDay) {
    setAvailableDays((currentDays) => {
      if (currentDays.includes(day)) {
        return currentDays.filter((currentDay) => currentDay !== day);
      }

      return [...currentDays, day];
    });
  }

  function handleSave() {
    updateProfile.mutate({
      preferredWorkoutsPerWeek: Number(preferredWorkoutsPerWeek),
      trainingDayPreferenceMode,
      availableDays:
        trainingDayPreferenceMode === "SelectedDays" ? availableDays : [],
    });
  }

  return (
    <>
      <section className="card">
        <h3>Profile settings</h3>

        <div className="form-grid">
          <label className="field">
            <span>Preferred workouts per week</span>
            <input
              type="number"
              min="1"
              max="6"
              value={preferredWorkoutsPerWeek}
              onChange={(event) =>
                setPreferredWorkoutsPerWeek(event.target.value)
              }
            />
          </label>

          <label className="field">
            <span>Training day mode</span>
            <select
              value={trainingDayPreferenceMode}
              onChange={(event) =>
                setTrainingDayPreferenceMode(
                  event.target.value as TrainingDayPreferenceMode,
                )
              }
            >
              <option value="AnyDay">Any day</option>
              <option value="SelectedDays">Selected days</option>
            </select>
          </label>
        </div>

        {trainingDayPreferenceMode === "SelectedDays" && (
          <div style={{ marginTop: 16 }}>
            <p className="field-label">Available days</p>

            <div className="checkbox-grid">
              {trainingDays.map((day) => (
                <label key={day} className="checkbox-card">
                  <input
                    type="checkbox"
                    checked={availableDays.includes(day)}
                    onChange={() => toggleDay(day)}
                  />
                  <span>{day}</span>
                </label>
              ))}
            </div>
          </div>
        )}

        <div className="actions">
          <button
            type="button"
            onClick={handleSave}
            disabled={updateProfile.isPending}
          >
            {updateProfile.isPending ? "Saving..." : "Save profile"}
          </button>
        </div>

        {updateProfile.error && (
          <p className="error">{updateProfile.error.message}</p>
        )}

        {updateProfile.data && <p className="success">Profile saved.</p>}
      </section>

      <section className="card">
        <h3>Current profile</h3>

        <p>
          <strong>{profile.preferredWorkoutsPerWeek}</strong> workouts per week
        </p>

        <p className="muted">Mode: {profile.trainingDayPreferenceMode}</p>

        <p className="muted">
          Available days:{" "}
          {profile.availableDays.length > 0
            ? profile.availableDays.join(", ")
            : "-"}
        </p>
      </section>
    </>
  );
}