using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.DocAPI.Migrations
{
    /// <inheritdoc />
    public partial class DocAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "T_Documents_Commerciaux",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "T_Documents_Commerciaux",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "T_Documents_Commerciaux");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "T_Documents_Commerciaux");
        }
    }
}
