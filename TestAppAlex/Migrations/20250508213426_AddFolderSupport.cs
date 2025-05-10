using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAppAlex.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FolderName",
                table: "Files",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderName",
                table: "Files");
        }
    }
}
