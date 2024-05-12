using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class Add_Product_stage_Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TemplateStage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(155)",
                oldMaxLength: 155,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageName",
                table: "ProductStage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "StageName",
                table: "ProductStage");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TemplateStage",
                type: "nvarchar(155)",
                maxLength: 155,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
