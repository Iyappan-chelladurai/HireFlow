using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class statusrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusName",
                table: "MST_ApplicationStatus",
                newName: "ApplicationStatusName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "MST_ApplicationStatus",
                newName: "ApplicationDescription");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "MST_ApplicationStatus",
                newName: "ApplicationStatusId");

            migrationBuilder.RenameColumn(
                name: "HistoryId",
                table: "JobApplicationStatusHistory",
                newName: "JobApplicationHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApplicationStatusName",
                table: "MST_ApplicationStatus",
                newName: "StatusName");

            migrationBuilder.RenameColumn(
                name: "ApplicationDescription",
                table: "MST_ApplicationStatus",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ApplicationStatusId",
                table: "MST_ApplicationStatus",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "JobApplicationHistoryId",
                table: "JobApplicationStatusHistory",
                newName: "HistoryId");
        }
    }
}
