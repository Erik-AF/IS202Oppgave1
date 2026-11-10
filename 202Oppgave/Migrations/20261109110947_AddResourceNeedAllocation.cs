using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _202Oppgave.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceNeedAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NeedId",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_NeedId",
                table: "Resources",
                column: "NeedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Needs_NeedId",
                table: "Resources",
                column: "NeedId",
                principalTable: "Needs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Needs_NeedId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_NeedId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "NeedId",
                table: "Resources");
        }
    }
}
