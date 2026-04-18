using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlindMatchPAS.Migrations
{
    /// <inheritdoc />
    public partial class FixedResearchAreaDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
