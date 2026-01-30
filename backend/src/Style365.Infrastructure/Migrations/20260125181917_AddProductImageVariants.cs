using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Style365.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImageVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ProductImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "ProductImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "S3Key",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailLargeS3Key",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailLargeUrl",
                table: "ProductImages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailMediumS3Key",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailMediumUrl",
                table: "ProductImages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailSmallS3Key",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailSmallUrl",
                table: "ProductImages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebPS3Key",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebPUrl",
                table: "ProductImages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "ProductImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_S3Key",
                table: "ProductImages",
                column: "S3Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_S3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "S3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailLargeS3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailLargeUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailMediumS3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailMediumUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailSmallS3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ThumbnailSmallUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "WebPS3Key",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "WebPUrl",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ProductImages");
        }
    }
}
