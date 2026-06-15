using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainFollowPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourcePlanId",
                table: "TripFollowers",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "TrainFollowPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TrainId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DaysOfWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    TargetStopId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AlertLeadTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainFollowPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainFollowPlans_Stops_TargetStopId",
                        column: x => x.TargetStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainFollowPlans_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainFollowPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TripFollowers_SourcePlanId",
                table: "TripFollowers",
                column: "SourcePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainFollowPlans_TargetStopId",
                table: "TrainFollowPlans",
                column: "TargetStopId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainFollowPlans_TrainId",
                table: "TrainFollowPlans",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainFollowPlans_UserId_TrainId",
                table: "TrainFollowPlans",
                columns: new[] { "UserId", "TrainId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TripFollowers_TrainFollowPlans_SourcePlanId",
                table: "TripFollowers",
                column: "SourcePlanId",
                principalTable: "TrainFollowPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripFollowers_TrainFollowPlans_SourcePlanId",
                table: "TripFollowers");

            migrationBuilder.DropTable(
                name: "TrainFollowPlans");

            migrationBuilder.DropIndex(
                name: "IX_TripFollowers_SourcePlanId",
                table: "TripFollowers");

            migrationBuilder.DropColumn(
                name: "SourcePlanId",
                table: "TripFollowers");
        }
    }
}
