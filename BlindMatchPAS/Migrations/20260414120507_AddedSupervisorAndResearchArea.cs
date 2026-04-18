using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlindMatchPAS.Migrations
{
    /// <inheritdoc />
    public partial class AddedSupervisorAndResearchArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResearchAreaId",
                table: "ProjectProposals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResearchAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supervisors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResearchAreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supervisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supervisors_ResearchAreas_ResearchAreaId",
                        column: x => x.ResearchAreaId,
                        principalTable: "ResearchAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_ResearchAreaId",
                table: "Supervisors",
                column: "ResearchAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.DropTable(
                name: "Supervisors");

            migrationBuilder.DropTable(
                name: "ResearchAreas");

            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "ResearchAreaId",
                table: "ProjectProposals");
        }
    }
}
