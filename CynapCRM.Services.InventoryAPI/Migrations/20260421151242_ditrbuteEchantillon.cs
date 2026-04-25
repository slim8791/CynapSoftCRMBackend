using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ditrbuteEchantillon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_Stock",
                table: "Distributions_Echantillons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Distributions_Echantillons_Id_Pharmacien",
                table: "Distributions_Echantillons",
                column: "Id_Pharmacien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Distributions_Echantillons_Id_Pharmacien",
                table: "Distributions_Echantillons");

            migrationBuilder.DropColumn(
                name: "Id_Stock",
                table: "Distributions_Echantillons");
        }
    }
}
