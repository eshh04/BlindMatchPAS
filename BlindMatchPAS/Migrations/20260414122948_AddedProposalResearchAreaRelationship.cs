using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlindMatchPAS.Migrations
{
    /// <inheritdoc />
    public partial class AddedProposalResearchAreaRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "ResearchArea",
                table: "ProjectProposals");

            migrationBuilder.AlterColumn<int>(
                name: "ResearchAreaId",
                table: "ProjectProposals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals");

            migrationBuilder.AlterColumn<int>(
                name: "ResearchAreaId",
                table: "ProjectProposals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ResearchArea",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProposals_ResearchAreas_ResearchAreaId",
                table: "ProjectProposals",
                column: "ResearchAreaId",
                principalTable: "ResearchAreas",
                principalColumn: "Id");
        }
    }
}
