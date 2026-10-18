using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSudokuModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SudokuScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Difficulty = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeSeconds = table.Column<int>(type: "int", nullable: false),
                    IsPersonalBest = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SudokuScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SudokuScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SudokuSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Difficulty = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PuzzleIndex = table.Column<int>(type: "int", nullable: false),
                    CellState = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ElapsedSeconds = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDiscarded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SudokuSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SudokuSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SudokuScores_Difficulty_TimeSeconds",
                table: "SudokuScores",
                columns: new[] { "Difficulty", "TimeSeconds" });

            migrationBuilder.CreateIndex(
                name: "IX_SudokuScores_UserId_Difficulty",
                table: "SudokuScores",
                columns: new[] { "UserId", "Difficulty" });

            migrationBuilder.CreateIndex(
                name: "IX_SudokuSessions_UserId_IsCompleted_IsDiscarded",
                table: "SudokuSessions",
                columns: new[] { "UserId", "IsCompleted", "IsDiscarded" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SudokuScores");

            migrationBuilder.DropTable(
                name: "SudokuSessions");
        }
    }
}
