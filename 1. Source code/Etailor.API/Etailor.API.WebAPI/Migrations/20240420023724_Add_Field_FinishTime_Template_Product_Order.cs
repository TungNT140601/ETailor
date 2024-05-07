using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_FinishTime_Template_Product_Order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AveDateForComplete",
                table: "ProductTemplate",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedTime",
                table: "Product",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishTime",
                table: "Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedTime",
                table: "Order",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AveDateForComplete",
                table: "ProductTemplate");

            migrationBuilder.DropColumn(
                name: "PlannedTime",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PlannedTime",
                table: "Order");
        }
    }
}
