using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Abstract", "IsRevealed", "ResearchAreaId", "StudentName", "SupervisorId", "Title" },
                values: new object[,]
                {
                    { 1, "Developing an AI system to assist in code reviews", false, 1, "John Doe", 1, "AI-Powered Code Review System" },
                    { 2, "Analyzing security vulnerabilities in blockchain implementations", true, 2, "Jane Smith", 2, "Blockchain Security Analysis" },
                    { 3, "Optimizing React applications for better performance", false, 3, "Bob Wilson", 3, "React Performance Optimization" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
