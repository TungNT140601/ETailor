using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class ChangeChatReferenceToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Chat__CustomerId__125EB334",
                table: "Chat");

            migrationBuilder.DropTable(
                name: "ChatHistory");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Chat",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_CustomerId",
                table: "Chat",
                newName: "IX_Chat_OrderId");

            migrationBuilder.CreateTable(
                name: "ChatList",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReplierId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FromCus = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    SendTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    ReadTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatList", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ChatHisto__ChatI__162F4418",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ChatHisto__Repli__17236851",
                        column: x => x.ReplierId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatList_ChatId",
                table: "ChatList",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatList_ReplierId",
                table: "ChatList",
                column: "ReplierId");

            migrationBuilder.AddForeignKey(
                name: "FK__Chat__OrderId__125EB334",
                table: "Chat",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Chat__OrderId__125EB334",
                table: "Chat");

            migrationBuilder.DropTable(
                name: "ChatList");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Chat",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Chat_OrderId",
                table: "Chat",
                newName: "IX_Chat_CustomerId");

            migrationBuilder.CreateTable(
                name: "ChatHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReplierId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FromCus = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    InactiveTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((0))"),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReadTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    SendTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ChatHisto__ChatI__162F4418",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__ChatHisto__Repli__17236851",
                        column: x => x.ReplierId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_ChatId",
                table: "ChatHistory",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_ReplierId",
                table: "ChatHistory",
                column: "ReplierId");

            migrationBuilder.AddForeignKey(
                name: "FK__Chat__CustomerId__125EB334",
                table: "Chat",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");
        }
    }
}
