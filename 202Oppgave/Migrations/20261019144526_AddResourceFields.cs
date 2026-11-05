using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _202Oppgave.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "Resources",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableTo",
                table: "Resources",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Resources",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Resources",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Resources",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Resources",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Resources",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "AvailableTo",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Resources");
        }
    }
}
