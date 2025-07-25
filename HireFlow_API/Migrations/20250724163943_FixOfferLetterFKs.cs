using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class FixOfferLetterFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Openings = table.Column<int>(type: "int", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_Jobs_UserAccounts_PostedBy",
                        column: x => x.PostedBy,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingStatusTable",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPersonalInfo_Completed = table.Column<bool>(type: "bit", nullable: false),
                    IsDocuments_Uploaded = table.Column<bool>(type: "bit", nullable: false),
                    IsDocuments_Verified = table.Column<bool>(type: "bit", nullable: false),
                    IsOfferLetter_Accepted = table.Column<bool>(type: "bit", nullable: false),
                    IsWelcomeEmail_Sent = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingStatusTable", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_OnboardingStatusTable_CandidateDetails_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateDetails",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResumePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AppliedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterviewFeedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OfferSentOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOfferAccepted = table.Column<bool>(type: "bit", nullable: false),
                    OnboardedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_JobApplications_CandidateDetails_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateDetails",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferLetters",
                columns: table => new
                {
                    OfferLetterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferLetterPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GeneratedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    AcceptedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferLetters", x => x.OfferLetterId);
                    table.ForeignKey(
                        name: "FK_OfferLetters_CandidateDetails_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateDetails",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferLetters_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId",
                table: "JobApplications",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId",
                table: "JobApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostedBy",
                table: "Jobs",
                column: "PostedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_CandidateId",
                table: "OfferLetters",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferLetters_JobId",
                table: "OfferLetters",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingStatusTable_CandidateId",
                table: "OnboardingStatusTable",
                column: "CandidateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "OfferLetters");

            migrationBuilder.DropTable(
                name: "OnboardingStatusTable");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
