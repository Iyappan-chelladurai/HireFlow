using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireFlow_API.Migrations
{
    /// <inheritdoc />
    public partial class intdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsAccountActive = table.Column<bool>(type: "bit", nullable: false),
                    AccountCreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CandidateDetails",
                columns: table => new
                {
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResumePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CurrentJobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalExperienceYears = table.Column<float>(type: "real", nullable: false),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfileCreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateDetails", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_CandidateDetails_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxFileSizeMB = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsVerificationRequired = table.Column<bool>(type: "bit", nullable: false),
                    FraudCheckLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                    table.ForeignKey(
                        name: "FK_DocumentTypes_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateDocumentDetails",
                columns: table => new
                {
                    DocumentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FileSizeInMB = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsFraudDetected = table.Column<bool>(type: "bit", nullable: false),
                    FraudScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateDocumentDetails", x => x.DocumentDetailId);
                    table.ForeignKey(
                        name: "FK_CandidateDocumentDetails_CandidateDetails_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateDetails",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateDocumentDetails_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDetails_UserId",
                table: "CandidateDetails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocumentDetails_CandidateId",
                table: "CandidateDocumentDetails",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocumentDetails_DocumentTypeId",
                table: "CandidateDocumentDetails",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_UserId",
                table: "DocumentTypes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateDocumentDetails");

            migrationBuilder.DropTable(
                name: "CandidateDetails");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
