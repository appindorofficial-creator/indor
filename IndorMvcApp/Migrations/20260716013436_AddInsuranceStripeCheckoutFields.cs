using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceStripeCheckoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeCheckoutSessionId",
                table: "IndorProviderInsuranceQuotes",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "IndorProviderInsuranceQuotes",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeCheckoutSessionId",
                table: "IndorProviderInsuranceQuotes");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "IndorProviderInsuranceQuotes");
        }
    }
}
