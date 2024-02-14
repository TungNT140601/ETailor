using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etailor.API.WebAPI.Migrations
{
    public partial class update_table_template_body_size : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InactiveTime",
                table: "TemplateBodySize",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TemplateBodySize",
                type: "bit",
                nullable: true,
                defaultValueSql: "((1))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InactiveTime",
                table: "TemplateBodySize");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TemplateBodySize");
        }
    }
}
