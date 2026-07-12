using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodFeed : Migration
    {
        // NOTE: This migration intentionally only creates the INDOR Neighborhood
        // feed tables. Unrelated *Es localization columns that EF detected as
        // pending (they are managed via Scripts/*.sql and already exist in
        // deployed databases) were removed to keep Update-Database safe.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LocationLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodPosts_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodComments_IndorNeighborhoodPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "IndorNeighborhoodPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodPostLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodPostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodPostLikes_IndorNeighborhoodPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "IndorNeighborhoodPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodPostSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodPostSaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodPostSaves_IndorNeighborhoodPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "IndorNeighborhoodPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodComments_PostId_CreatedUtc",
                table: "IndorNeighborhoodComments",
                columns: new[] { "PostId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodPostLikes_PostId_UserId",
                table: "IndorNeighborhoodPostLikes",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodPosts_PropiedadId",
                table: "IndorNeighborhoodPosts",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodPosts_ZipCode_IsActive_CreatedUtc",
                table: "IndorNeighborhoodPosts",
                columns: new[] { "ZipCode", "IsActive", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodPostSaves_PostId_UserId",
                table: "IndorNeighborhoodPostSaves",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndorNeighborhoodComments");

            migrationBuilder.DropTable(
                name: "IndorNeighborhoodPostLikes");

            migrationBuilder.DropTable(
                name: "IndorNeighborhoodPostSaves");

            migrationBuilder.DropTable(
                name: "IndorNeighborhoodPosts");
        }
    }
}
