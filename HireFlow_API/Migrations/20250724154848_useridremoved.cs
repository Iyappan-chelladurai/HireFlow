using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class useridremoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_UserAccounts_UserId",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_UserId",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DocumentTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "DocumentTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_UserId",
                table: "DocumentTypes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_UserAccounts_UserId",
                table: "DocumentTypes",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
