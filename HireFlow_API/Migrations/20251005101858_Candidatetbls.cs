using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class Candidatetbls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "CandidateDetails");

            migrationBuilder.RenameColumn(
                name: "ResumePath",
                table: "CandidateDetails",
                newName: "EducationLevel");

            migrationBuilder.AddColumn<int>(
                name: "NoticePeriodDays",
                table: "CandidateDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "CandidateDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoticePeriodDays",
                table: "CandidateDetails");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "CandidateDetails");

            migrationBuilder.RenameColumn(
                name: "EducationLevel",
                table: "CandidateDetails",
                newName: "ResumePath");

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "CandidateDetails",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
