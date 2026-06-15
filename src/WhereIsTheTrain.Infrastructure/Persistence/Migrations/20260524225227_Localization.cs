using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Localization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Stops");

            migrationBuilder.RenameColumn(
                name: "RouteDescription",
                table: "TrainSuggestions",
                newName: "RouteDescriptionEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TrainSuggestions",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TrainSuggestions",
                newName: "RouteDescriptionAr");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Trains",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Trains",
                newName: "DescriptionEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Stops",
                newName: "DescriptionEn");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Stops",
                newName: "CityEn");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "TrainSuggestions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "TrainSuggestions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "TrainSuggestions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Trains",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Trains",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CityAr",
                table: "Stops",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Stops",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Stops",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Stops",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "TrainSuggestions");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "TrainSuggestions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "TrainSuggestions");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "CityAr",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Stops");

            migrationBuilder.RenameColumn(
                name: "RouteDescriptionEn",
                table: "TrainSuggestions",
                newName: "RouteDescription");

            migrationBuilder.RenameColumn(
                name: "RouteDescriptionAr",
                table: "TrainSuggestions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "TrainSuggestions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Trains",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "Trains",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "Stops",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CityEn",
                table: "Stops",
                newName: "City");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Stops",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
