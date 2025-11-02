using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class statushistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InterviewFeedback",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MST_ApplicationStatus",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MST_ApplicationStatus", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationStatusHistory",
                columns: table => new
                {
                    HistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationStatusHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_JobApplicationStatusHistory_JobApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobApplicationStatusHistory_MST_ApplicationStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "MST_ApplicationStatus",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_ApplicationId",
                table: "JobApplicationStatusHistory",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_StatusId",
                table: "JobApplicationStatusHistory",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplicationStatusHistory");

            migrationBuilder.DropTable(
                name: "MST_ApplicationStatus");

            migrationBuilder.AlterColumn<string>(
                name: "InterviewFeedback",
                table: "JobApplications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
