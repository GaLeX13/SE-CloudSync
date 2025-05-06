using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAppAlex.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToFileItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Files");
        }
    }
}
