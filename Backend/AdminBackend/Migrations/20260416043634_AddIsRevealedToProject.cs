using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRevealedToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevealed",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevealed",
                table: "Projects");
        }
    }
}
