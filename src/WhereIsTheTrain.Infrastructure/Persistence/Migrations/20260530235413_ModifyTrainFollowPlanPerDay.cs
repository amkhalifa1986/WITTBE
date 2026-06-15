using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTrainFollowPlanPerDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "TrainFollowPlans");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "TrainFollowPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrainFollowPlans_UserId_TrainId_DayOfWeek",
                table: "TrainFollowPlans",
                columns: new[] { "UserId", "TrainId", "DayOfWeek" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_TrainFollowPlans_UserId_TrainId",
                table: "TrainFollowPlans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                table: "TrainFollowPlans",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TrainFollowPlans_UserId_TrainId",
                table: "TrainFollowPlans",
                columns: new[] { "UserId", "TrainId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_TrainFollowPlans_UserId_TrainId_DayOfWeek",
                table: "TrainFollowPlans");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "TrainFollowPlans");
        }
    }
}
