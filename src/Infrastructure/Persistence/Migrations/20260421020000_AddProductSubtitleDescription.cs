using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodHamburger.Infrastructure.Persistence.Migrations;

public partial class AddProductSubtitleDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Subtitle",
            table: "Products",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Products",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        // Update seed products with their meta data
        migrationBuilder.Sql(@"
            UPDATE ""Products"" SET ""Subtitle"" = 'pão + burger + queijo', ""Description"" = 'O original.'          WHERE ""Id"" = '11111111-0000-0000-0000-000000000001';
            UPDATE ""Products"" SET ""Subtitle"" = 'pão + burger + ovo',    ""Description"" = 'Gema escorrendo.'       WHERE ""Id"" = '11111111-0000-0000-0000-000000000002';
            UPDATE ""Products"" SET ""Subtitle"" = 'pão + burger + bacon',  ""Description"" = 'Favorito da casa.'      WHERE ""Id"" = '11111111-0000-0000-0000-000000000003';
            UPDATE ""Products"" SET ""Subtitle"" = 'porção crocante',       ""Description"" = 'Corte rústico, sal grosso.' WHERE ""Id"" = '11111111-0000-0000-0000-000000000004';
            UPDATE ""Products"" SET ""Subtitle"" = 'lata 350ml',            ""Description"" = 'Gelado.'                WHERE ""Id"" = '11111111-0000-0000-0000-000000000005';
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Subtitle",     table: "Products");
        migrationBuilder.DropColumn(name: "Description",  table: "Products");
    }
}
