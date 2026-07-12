using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodPostTypeAndReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Audience",
                table: "IndorNeighborhoodPosts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "PostType",
                table: "IndorNeighborhoodPosts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "IndorNeighborhoodComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "IndorNeighborhoodComments",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborhoodComments_ParentCommentId",
                table: "IndorNeighborhoodComments",
                column: "ParentCommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndorNeighborhoodComments_ParentCommentId",
                table: "IndorNeighborhoodComments");

            migrationBuilder.DropColumn(
                name: "Audience",
                table: "IndorNeighborhoodPosts");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "IndorNeighborhoodPosts");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "IndorNeighborhoodComments");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "IndorNeighborhoodComments");
        }
    }
}
