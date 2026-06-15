using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRailwayPathDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "RailwayPaths",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "RailwayPaths",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "RailwayPaths",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Seed existing paths with unique codes and names
            migrationBuilder.Sql("UPDATE RailwayPaths SET Code = 'CA-AS', NameEn = 'Cairo - Asyut', NameAr = 'القاهرة - أسيوط' WHERE StartStationId = '00000000-0000-0000-0000-000000000001' AND EndStationId = '00000000-0000-0000-0000-000000000013';");
            migrationBuilder.Sql("UPDATE RailwayPaths SET Code = 'AL-CA', NameEn = 'Alexandria - Cairo', NameAr = 'الإسكندرية - القاهرة' WHERE StartStationId = '00000000-0000-0000-0000-000000000003' AND EndStationId = '00000000-0000-0000-0000-000000000001';");
            migrationBuilder.Sql("UPDATE RailwayPaths SET Code = 'AS-QE', NameEn = 'Asyut - Qena', NameAr = 'أسيوط - قنا' WHERE StartStationId = '00000000-0000-0000-0000-000000000013' AND EndStationId = '00000000-0000-0000-0000-000000000016';");
            migrationBuilder.Sql("UPDATE RailwayPaths SET Code = 'QE-AW', NameEn = 'Qena - Aswan', NameAr = 'قنا - أسوان' WHERE StartStationId = '00000000-0000-0000-0000-000000000016' AND EndStationId = '00000000-0000-0000-0000-000000000018';");
            migrationBuilder.Sql("UPDATE RailwayPaths SET Code = UUID() WHERE Code = '';");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayPaths_Code",
                table: "RailwayPaths",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RailwayPaths_Code",
                table: "RailwayPaths");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "RailwayPaths");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "RailwayPaths");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "RailwayPaths");
        }
    }
}
