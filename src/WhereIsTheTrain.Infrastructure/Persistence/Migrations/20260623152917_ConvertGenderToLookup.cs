using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertGenderToLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "GenderLookups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NameEn = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameAr = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenderLookups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GenderId",
                table: "Users",
                column: "GenderId");

            migrationBuilder.Sql("INSERT INTO GenderLookups (Id, NameEn, NameAr) VALUES ('00000000-0000-0000-0000-000000003001', 'Male', 'ذكر'), ('00000000-0000-0000-0000-000000003002', 'Female', 'أنثى');");
            migrationBuilder.Sql("UPDATE Users SET GenderId = '00000000-0000-0000-0000-000000003001';");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_GenderLookups_GenderId",
                table: "Users",
                column: "GenderId",
                principalTable: "GenderLookups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_GenderLookups_GenderId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "GenderLookups");

            migrationBuilder.DropIndex(
                name: "IX_Users_GenderId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
