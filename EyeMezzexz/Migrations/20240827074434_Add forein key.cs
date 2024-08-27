using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class Addforeinkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationUserId",
                table: "StaffInOut",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffInOut_ApplicationUserId",
                table: "StaffInOut",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffInOut_AspNetUsers_ApplicationUserId",
                table: "StaffInOut",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffInOut_AspNetUsers_ApplicationUserId",
                table: "StaffInOut");

            migrationBuilder.DropIndex(
                name: "IX_StaffInOut_ApplicationUserId",
                table: "StaffInOut");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "StaffInOut");
        }
    }
}
