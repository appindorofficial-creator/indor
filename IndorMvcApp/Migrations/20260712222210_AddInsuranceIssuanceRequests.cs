using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceIssuanceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndorInsuranceIssuanceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    RequestCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BusinessAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    WorkersComp = table.Column<bool>(type: "bit", nullable: false),
                    GeneralLiability = table.Column<bool>(type: "bit", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    OwnerDateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    OwnerPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OwnerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TypeOfBusiness = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    NumberOfEmployees = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    EmployeePayroll = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompanyGross = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CarrierEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CarrierEmailStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CarrierEmailSentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorInsuranceIssuanceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorInsuranceIssuanceRequests_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndorInsuranceIssuanceRequests_ProveedorId_CreatedUtc",
                table: "IndorInsuranceIssuanceRequests",
                columns: new[] { "ProveedorId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IndorInsuranceIssuanceRequests_RequestCode",
                table: "IndorInsuranceIssuanceRequests",
                column: "RequestCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndorInsuranceIssuanceRequests");
        }
    }
}
