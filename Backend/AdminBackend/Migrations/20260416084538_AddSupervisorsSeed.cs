using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Supervisors",
                columns: new[] { "Id", "Department", "Name" },
                values: new object[,]
                {
                    { 1, "Computer Science", "Dr. Alice Johnson" },
                    { 2, "Information Security", "Prof. Bob Smith" },
                    { 3, "Software Engineering", "Dr. Carol Williams" },
                    { 4, "Data Science", "Prof. David Brown" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Supervisors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Supervisors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Supervisors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Supervisors",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
