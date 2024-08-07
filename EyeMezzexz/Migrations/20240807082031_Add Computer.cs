using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddComputer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComputerId",
                table: "TaskNames",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Computers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Computers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskNames_ComputerId",
                table: "TaskNames",
                column: "ComputerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskNames_Computers_ComputerId",
                table: "TaskNames",
                column: "ComputerId",
                principalTable: "Computers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskNames_Computers_ComputerId",
                table: "TaskNames");

            migrationBuilder.DropTable(
                name: "Computers");

            migrationBuilder.DropIndex(
                name: "IX_TaskNames_ComputerId",
                table: "TaskNames");

            migrationBuilder.DropColumn(
                name: "ComputerId",
                table: "TaskNames");
        }
    }
}
