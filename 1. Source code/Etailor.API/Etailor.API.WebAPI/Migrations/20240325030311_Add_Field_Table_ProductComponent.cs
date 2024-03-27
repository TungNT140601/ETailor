using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Field_Table_ProductComponent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerClient");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ProductComponent",
                type: "nvarchar(2550)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteImage",
                table: "ProductComponent",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "ProductComponent");

            migrationBuilder.DropColumn(
                name: "NoteImage",
                table: "ProductComponent");

            migrationBuilder.CreateTable(
                name: "CustomerClient",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClientToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK__CustomerC__Custo__2B2A60FE",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerClient_CustomerId",
                table: "CustomerClient",
                column: "CustomerId");
        }
    }
}
