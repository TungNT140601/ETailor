using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class add_field_image_productstage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceImage",
                table: "ProductStage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "ChatList",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceImage",
                table: "ProductStage");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "ChatList");
        }
    }
}
