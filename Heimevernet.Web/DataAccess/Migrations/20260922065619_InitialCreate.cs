using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Heimevernet.Web.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS `Resources` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
                    `Type` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    CONSTRAINT `PK_Resources` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
