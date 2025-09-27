using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class JobScoreCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_CandidatesJobScore",
                columns: table => new
                {
                    ScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResumeMatchScore = table.Column<int>(type: "int", nullable: false),
                    SkillsMatchScore = table.Column<int>(type: "int", nullable: false),
                    ExperienceScore = table.Column<int>(type: "int", nullable: false),
                    OverallFitScore = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EvaluatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CandidatesJobScore", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_tbl_CandidatesJobScore_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CandidatesJobScore_JobApplicationId",
                table: "tbl_CandidatesJobScore",
                column: "JobApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_CandidatesJobScore");
        }
    }
}
