using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodMediaAndCommentSaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodCommentSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodCommentSaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodCommentSaves_IndorNeighborhoodComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "IndorNeighborhoodComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborhoodPostMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborhoodPostMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborhoodPostMedia_IndorNeighborhoodPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "IndorNeighborhoodPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodCommentSaves_CommentId_UserId",
                table: "IndorNeighborhoodCommentSaves",
                columns: new[] { "CommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodPostMedia_PostId_SortOrder",
                table: "IndorNeighborhoodPostMedia",
                columns: new[] { "PostId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndorNeighborhoodCommentSaves");

            migrationBuilder.DropTable(
                name: "IndorNeighborhoodPostMedia");
        }
    }
}
