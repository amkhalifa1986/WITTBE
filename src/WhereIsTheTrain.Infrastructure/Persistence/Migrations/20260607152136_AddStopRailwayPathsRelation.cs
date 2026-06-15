using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStopRailwayPathsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RailwayPathStops",
                columns: table => new
                {
                    RailwayPathsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StopsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RailwayPathStops", x => new { x.RailwayPathsId, x.StopsId });
                    table.ForeignKey(
                        name: "FK_RailwayPathStops_RailwayPaths_RailwayPathsId",
                        column: x => x.RailwayPathsId,
                        principalTable: "RailwayPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RailwayPathStops_Stops_StopsId",
                        column: x => x.StopsId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RailwayPathStops_StopsId",
                table: "RailwayPathStops",
                column: "StopsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RailwayPathStops");
        }
    }
}
