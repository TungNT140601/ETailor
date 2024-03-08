using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_Table_Payment_Product : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffMakerId",
                table: "Product",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffCreateId",
                table: "Payment",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_StaffMakerId",
                table: "Product",
                column: "StaffMakerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_StaffCreateId",
                table: "Payment",
                column: "StaffCreateId");

            migrationBuilder.AddForeignKey(
                name: "FK__Payment__Staff_Create__1A25A48",
                table: "Payment",
                column: "StaffCreateId",
                principalTable: "Staff",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__Product__Staff_Maker__6B44E613",
                table: "Product",
                column: "StaffMakerId",
                principalTable: "Staff",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Payment__Staff_Create__1A25A48",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK__Product__Staff_Maker__6B44E613",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_StaffMakerId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Payment_StaffCreateId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StaffMakerId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StaffCreateId",
                table: "Payment");
        }
    }
}
