using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebStoreMVC.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "UserReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDateTime",
                table: "UserReviews",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "UserReviews");

            migrationBuilder.DropColumn(
                name: "ReviewDateTime",
                table: "UserReviews");
        }
    }
}
