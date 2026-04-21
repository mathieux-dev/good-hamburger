using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodHamburger.Infrastructure.Persistence.Migrations;

public partial class AddOrderServiceType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ServiceType",
            table: "Orders",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Salão");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ServiceType", table: "Orders");
    }
}
