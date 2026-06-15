using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainNumberToAdAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainNumber",
                table: "AdImpressions",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TrainNumber",
                table: "AdClicks",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AdImpressions_TrainNumber",
                table: "AdImpressions",
                column: "TrainNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AdClicks_TrainNumber",
                table: "AdClicks",
                column: "TrainNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdImpressions_TrainNumber",
                table: "AdImpressions");

            migrationBuilder.DropIndex(
                name: "IX_AdClicks_TrainNumber",
                table: "AdClicks");

            migrationBuilder.DropColumn(
                name: "TrainNumber",
                table: "AdImpressions");

            migrationBuilder.DropColumn(
                name: "TrainNumber",
                table: "AdClicks");
        }
    }
}
