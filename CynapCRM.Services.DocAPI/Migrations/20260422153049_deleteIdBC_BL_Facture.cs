using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.DocAPI.Migrations
{
    /// <inheritdoc />
    public partial class deleteIdBC_BL_Facture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id_BC",
                table: "T_Documents_Commerciaux");

            migrationBuilder.DropColumn(
                name: "Id_BL",
                table: "T_Documents_Commerciaux");

            migrationBuilder.DropColumn(
                name: "Id_Facture",
                table: "T_Documents_Commerciaux");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_BC",
                table: "T_Documents_Commerciaux",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_BL",
                table: "T_Documents_Commerciaux",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_Facture",
                table: "T_Documents_Commerciaux",
                type: "int",
                nullable: true);
        }
    }
}
