using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MySQL: FK constraints require a backing index.
            // We must drop the FK, drop the old single-column index,
            // create the new composite index (MySQL will use it to back the FK),
            // then recreate the FK.

            // ── TripTelemetry: upgrade TripId → (TripId, Timestamp) ──────────────
            migrationBuilder.Sql(
                "ALTER TABLE `TripTelemetry` DROP FOREIGN KEY `FK_TripTelemetry_Trips_TripId`;");

            migrationBuilder.DropIndex(
                name: "IX_TripTelemetry_TripId",
                table: "TripTelemetry");

            // Composite index for fast tracking queries (polled every 10s per user)
            migrationBuilder.CreateIndex(
                name: "IX_TripTelemetry_TripId_Timestamp",
                table: "TripTelemetry",
                columns: new[] { "TripId", "Timestamp" });

            // Recreate FK — MySQL will use the composite index as its backing index
            migrationBuilder.Sql(
                "ALTER TABLE `TripTelemetry` ADD CONSTRAINT `FK_TripTelemetry_Trips_TripId` " +
                "FOREIGN KEY (`TripId`) REFERENCES `Trips` (`Id`) ON DELETE CASCADE;");

            // ── Trips: add TripDate index for GetTodayTrips ───────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_Trips_TripDate",
                table: "Trips",
                column: "TripDate");

            // ── Notifications: upgrade UserId → (UserId, IsRead) ─────────────────
            migrationBuilder.Sql(
                "ALTER TABLE `Notifications` DROP FOREIGN KEY `FK_Notifications_Users_UserId`;");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            // Composite index for notification badge queries (UserId + IsRead)
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            // Recreate FK backed by composite index
            migrationBuilder.Sql(
                "ALTER TABLE `Notifications` ADD CONSTRAINT `FK_Notifications_Users_UserId` " +
                "FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── TripTelemetry: rollback composite → single-column ─────────────────
            migrationBuilder.Sql(
                "ALTER TABLE `TripTelemetry` DROP FOREIGN KEY `FK_TripTelemetry_Trips_TripId`;");

            migrationBuilder.DropIndex(
                name: "IX_TripTelemetry_TripId_Timestamp",
                table: "TripTelemetry");

            migrationBuilder.CreateIndex(
                name: "IX_TripTelemetry_TripId",
                table: "TripTelemetry",
                column: "TripId");

            migrationBuilder.Sql(
                "ALTER TABLE `TripTelemetry` ADD CONSTRAINT `FK_TripTelemetry_Trips_TripId` " +
                "FOREIGN KEY (`TripId`) REFERENCES `Trips` (`Id`) ON DELETE CASCADE;");

            // ── Trips: remove TripDate index ──────────────────────────────────────
            migrationBuilder.DropIndex(
                name: "IX_Trips_TripDate",
                table: "Trips");

            // ── Notifications: rollback composite → single-column ─────────────────
            migrationBuilder.Sql(
                "ALTER TABLE `Notifications` DROP FOREIGN KEY `FK_Notifications_Users_UserId`;");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.Sql(
                "ALTER TABLE `Notifications` ADD CONSTRAINT `FK_Notifications_Users_UserId` " +
                "FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;");
        }
    }
}
