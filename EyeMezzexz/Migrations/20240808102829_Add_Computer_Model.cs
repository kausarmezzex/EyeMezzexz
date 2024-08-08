using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class Add_Computer_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Computers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Computers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifyBy",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyOn",
                table: "Computers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "ModifyBy",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "ModifyOn",
                table: "Computers");
        }
    }
}
