using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LineString>(
                name: "RoutePath",
                table: "Trains",
                type: "geometry",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistanceAlongRoute",
                table: "TrainRouteStops",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Stops",
                type: "geometry",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TripTelemetry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TripId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RawLatitude = table.Column<double>(type: "double", nullable: false),
                    RawLongitude = table.Column<double>(type: "double", nullable: false),
                    SnappedLatitude = table.Column<double>(type: "double", nullable: false),
                    SnappedLongitude = table.Column<double>(type: "double", nullable: false),
                    Speed = table.Column<double>(type: "double", nullable: false),
                    DistanceAlongRoute = table.Column<double>(type: "double", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripTelemetry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripTelemetry_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripTelemetry_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TripTelemetry_TripId",
                table: "TripTelemetry",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTelemetry_UserId",
                table: "TripTelemetry",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripTelemetry");

            migrationBuilder.DropColumn(
                name: "RoutePath",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "DistanceAlongRoute",
                table: "TrainRouteStops");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Stops");
        }
    }
}
