using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhereIsTheTrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripStatusLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0. Clean up any partial state from previous failed migration run
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND CONSTRAINT_NAME = 'FK_Trips_TripStatusLookups_StatusId') > 0,
    'ALTER TABLE Trips DROP FOREIGN KEY FK_Trips_TripStatusLookups_StatusId',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'StatusId') > 0,
    'ALTER TABLE Trips DROP COLUMN StatusId',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql("DROP TABLE IF EXISTS TripStatusLookups;");

            // 1. Create the lookup table
            migrationBuilder.CreateTable(
                name: "TripStatusLookups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameAr = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameEn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStatusLookups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TripStatusLookups_Code",
                table: "TripStatusLookups",
                column: "Code",
                unique: true);

            // 2. Seed lookups in the migration
            migrationBuilder.InsertData(
                table: "TripStatusLookups",
                columns: new[] { "Id", "Code", "NameAr", "NameEn", "Color" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000004001"), "Scheduled", "جدولة", "Scheduled", "#71717a" },
                    { new Guid("00000000-0000-0000-0000-000000004002"), "Departed", "تحرك", "Departed", "#3b82f6" },
                    { new Guid("00000000-0000-0000-0000-000000004003"), "InTransit", "في الطريق", "In Transit", "#6366f1" },
                    { new Guid("00000000-0000-0000-0000-000000004004"), "Arrived", "وصل", "Arrived", "#10b981" },
                    { new Guid("00000000-0000-0000-0000-000000004005"), "Cancelled", "ملغي", "Cancelled", "#ef4444" },
                    { new Guid("00000000-0000-0000-0000-000000004006"), "Delayed", "متأخر", "Delayed", "#f59e0b" }
                });

            // 3. Add StatusId as nullable initially to allow updates
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Trips",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            // 4. Map existing Status int to StatusId Guids conditionally
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004001\' WHERE Status = 0',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004002\' WHERE Status = 1',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004003\' WHERE Status = 2',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004004\' WHERE Status = 3',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004005\' WHERE Status = 4',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'UPDATE Trips SET StatusId = \'00000000-0000-0000-0000-000000004006\' WHERE Status = 5',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
            
            // Fallback for any invalid values
            migrationBuilder.Sql("UPDATE Trips SET StatusId = '00000000-0000-0000-0000-000000004001' WHERE StatusId IS NULL;");

            // 5. Make StatusId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "StatusId",
                table: "Trips",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci");

            // 6. Drop old Status column conditionally
            migrationBuilder.Sql(@"
SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Trips' AND COLUMN_NAME = 'Status') > 0,
    'ALTER TABLE Trips DROP COLUMN Status',
    'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // 7. Add index and foreign key constraint
            migrationBuilder.CreateIndex(
                name: "IX_Trips_StatusId",
                table: "Trips",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_TripStatusLookups_StatusId",
                table: "Trips",
                column: "StatusId",
                principalTable: "TripStatusLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_TripStatusLookups_StatusId",
                table: "Trips");

            // 1. Add back the old Status column as nullable first (to update values) or default to 0
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2. Map StatusId Guids back to Status int
            migrationBuilder.Sql("UPDATE Trips SET Status = 0 WHERE StatusId = '00000000-0000-0000-0000-000000004001';");
            migrationBuilder.Sql("UPDATE Trips SET Status = 1 WHERE StatusId = '00000000-0000-0000-0000-000000004002';");
            migrationBuilder.Sql("UPDATE Trips SET Status = 2 WHERE StatusId = '00000000-0000-0000-0000-000000004003';");
            migrationBuilder.Sql("UPDATE Trips SET Status = 3 WHERE StatusId = '00000000-0000-0000-0000-000000004004';");
            migrationBuilder.Sql("UPDATE Trips SET Status = 4 WHERE StatusId = '00000000-0000-0000-0000-000000004005';");
            migrationBuilder.Sql("UPDATE Trips SET Status = 5 WHERE StatusId = '00000000-0000-0000-0000-000000004006';");

            migrationBuilder.DropTable(
                name: "TripStatusLookups");

            migrationBuilder.DropIndex(
                name: "IX_Trips_StatusId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Trips");
        }
    }
}
