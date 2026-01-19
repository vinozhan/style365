using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Style365.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCartItemMoneyValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_ProductId_IsApproved",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ProductReviews");

            migrationBuilder.RenameColumn(
                name: "ApprovedAt",
                table: "ProductReviews",
                newName: "PublishedAt");

            migrationBuilder.AddColumn<int>(
                name: "HelpfulCount",
                table: "ProductReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModerationNotes",
                table: "ProductReviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotHelpfulCount",
                table: "ProductReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemId",
                table: "ProductReviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProductReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CartItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_OrderItemId",
                table: "ProductReviews",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId_Status",
                table: "ProductReviews",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_Status",
                table: "ProductReviews",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_OrderItems_OrderItemId",
                table: "ProductReviews",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_OrderItems_OrderItemId",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_OrderItemId",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_ProductId_Status",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_Status",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "HelpfulCount",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "ModerationNotes",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "NotHelpfulCount",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "PublishedAt",
                table: "ProductReviews",
                newName: "ApprovedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ProductReviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId_IsApproved",
                table: "ProductReviews",
                columns: new[] { "ProductId", "IsApproved" });
        }
    }
}
