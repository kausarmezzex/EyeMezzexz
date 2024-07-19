using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "demos");

            migrationBuilder.DropColumn(
                name: "ActivityLog",
                table: "UploadedData");

            migrationBuilder.DropColumn(
                name: "SystemInfo",
                table: "UploadedData");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "UploadedData",
                newName: "CreatedOn");

            migrationBuilder.AddColumn<string>(
                name: "TaskCreatedBy",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaskCreatedOn",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskModifiedBy",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaskModifiedOn",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskCreatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskCreatedOn",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskModifiedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskModifiedOn",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "UploadedData",
                newName: "Timestamp");

            migrationBuilder.AddColumn<string>(
                name: "ActivityLog",
                table: "UploadedData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SystemInfo",
                table: "UploadedData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "demos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demos", x => x.Id);
                });
        }
    }
}
