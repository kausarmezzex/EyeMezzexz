using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUploadedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "UploadedData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskTimerId",
                table: "UploadedData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedData_TaskTimerId",
                table: "UploadedData",
                column: "TaskTimerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedData_TaskTimers_TaskTimerId",
                table: "UploadedData",
                column: "TaskTimerId",
                principalTable: "TaskTimers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedData_TaskTimers_TaskTimerId",
                table: "UploadedData");

            migrationBuilder.DropIndex(
                name: "IX_UploadedData_TaskTimerId",
                table: "UploadedData");

            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "UploadedData");

            migrationBuilder.DropColumn(
                name: "TaskTimerId",
                table: "UploadedData");
        }
    }
}
