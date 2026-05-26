using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class GuardarPropiedadJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Propiedades");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Propiedades",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatosJson",
                table: "Propiedades",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Propiedades",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Propiedades_UserId",
                table: "Propiedades",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Propiedades_AspNetUsers_UserId",
                table: "Propiedades",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Propiedades_AspNetUsers_UserId",
                table: "Propiedades");

            migrationBuilder.DropIndex(
                name: "IX_Propiedades_UserId",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "DatosJson",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Propiedades");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Propiedades",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Propiedades",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Propiedades",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Propiedades",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
