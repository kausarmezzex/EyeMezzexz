using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientTimeZone",
                table: "UploadedData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientTimeZone",
                table: "TaskTimers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeDifference",
                table: "TaskTimers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientTimeZone",
                table: "StaffInOut",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeDifference",
                table: "StaffInOut",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientTimeZone",
                table: "UploadedData");

            migrationBuilder.DropColumn(
                name: "ClientTimeZone",
                table: "TaskTimers");

            migrationBuilder.DropColumn(
                name: "TimeDifference",
                table: "TaskTimers");

            migrationBuilder.DropColumn(
                name: "ClientTimeZone",
                table: "StaffInOut");

            migrationBuilder.DropColumn(
                name: "TimeDifference",
                table: "StaffInOut");
        }
    }
}
