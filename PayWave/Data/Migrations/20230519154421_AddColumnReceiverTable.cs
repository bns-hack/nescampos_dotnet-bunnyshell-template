using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayWave.Data.Migrations
{
    public partial class AddColumnReceiverTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Receivers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Receivers");
        }
    }
}
