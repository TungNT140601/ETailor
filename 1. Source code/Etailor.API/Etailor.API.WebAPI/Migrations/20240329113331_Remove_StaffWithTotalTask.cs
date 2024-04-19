using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Remove_StaffWithTotalTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffWithTotalTask",
                table: "StaffWithTotalTask");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "StaffWithTotalTask",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "OrderDashboard",
                type: "decimal(18,3)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "OrderDashboard");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "StaffWithTotalTask",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffWithTotalTask",
                table: "StaffWithTotalTask",
                column: "Id");
        }
    }
}
