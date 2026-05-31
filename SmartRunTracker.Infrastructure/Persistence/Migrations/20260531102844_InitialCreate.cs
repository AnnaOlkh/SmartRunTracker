using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartRunTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "runner_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PreferredWorkoutsPerWeek = table.Column<int>(type: "integer", nullable: false),
                    TrainingDayPreferenceMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runner_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runner_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "running_goals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TargetDistanceKm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TargetPaceSecondsPerKm = table.Column<int>(type: "integer", nullable: false),
                    GoalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_running_goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_running_goals_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runner_available_days",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunnerProfileId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runner_available_days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runner_available_days_runner_profiles_RunnerProfileId",
                        column: x => x.RunnerProfileId,
                        principalTable: "runner_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_weeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RunningGoalId = table.Column<int>(type: "integer", nullable: false),
                    WeekStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetWorkoutCount = table.Column<int>(type: "integer", nullable: false),
                    AdjustmentMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetWeekDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_weeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_weeks_running_goals_RunningGoalId",
                        column: x => x.RunningGoalId,
                        principalTable: "running_goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_training_weeks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planned_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingWeekId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Intensity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    TargetDistanceKm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    TargetPaceSecondsPerKm = table.Column<int>(type: "integer", nullable: true),
                    ScheduledFor = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planned_sessions_training_weeks_TrainingWeekId",
                        column: x => x.TrainingWeekId,
                        principalTable: "training_weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PlannedSessionId = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    Rpe = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workouts_planned_sessions_PlannedSessionId",
                        column: x => x.PlannedSessionId,
                        principalTable: "planned_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_workouts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planned_sessions_ScheduledFor",
                table: "planned_sessions",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_planned_sessions_TrainingWeekId",
                table: "planned_sessions",
                column: "TrainingWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_runner_available_days_RunnerProfileId_Day",
                table: "runner_available_days",
                columns: new[] { "RunnerProfileId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runner_profiles_UserId",
                table: "runner_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_running_goals_UserId",
                table: "running_goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_training_weeks_RunningGoalId",
                table: "training_weeks",
                column: "RunningGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_training_weeks_UserId_WeekStartDate",
                table: "training_weeks",
                columns: new[] { "UserId", "WeekStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workouts_PlannedSessionId",
                table: "workouts",
                column: "PlannedSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workouts_StartedAt",
                table: "workouts",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_workouts_UserId",
                table: "workouts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "runner_available_days");

            migrationBuilder.DropTable(
                name: "workouts");

            migrationBuilder.DropTable(
                name: "runner_profiles");

            migrationBuilder.DropTable(
                name: "planned_sessions");

            migrationBuilder.DropTable(
                name: "training_weeks");

            migrationBuilder.DropTable(
                name: "running_goals");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
