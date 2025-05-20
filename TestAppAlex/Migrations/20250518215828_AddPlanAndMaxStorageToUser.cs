using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAppAlex.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanAndMaxStorageToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxStorageMB",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStorageMB",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Users");
        }
    }
}
