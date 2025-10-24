using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class InterviewTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InterviewMode",
                table: "InterviewScheduleDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "InterviewScheduleDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleReason",
                table: "InterviewScheduleDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduledDate",
                table: "InterviewScheduleDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "InterviewScheduleDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "InterviewScheduleDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InterviewScheduleDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "InterviewScheduleDetails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "RescheduleReason",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "RescheduledDate",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InterviewScheduleDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "InterviewScheduleDetails");

            migrationBuilder.AlterColumn<string>(
                name: "InterviewMode",
                table: "InterviewScheduleDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
