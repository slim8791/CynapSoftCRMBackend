using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distributions_Echantillons",
                columns: table => new
                {
                    Id_Distribution = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Qte = table.Column<int>(type: "int", nullable: false),
                    DateDistribution = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Medecin = table.Column<int>(type: "int", nullable: false),
                    NumeroLot = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributions_Echantillons", x => x.Id_Distribution);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id_stock = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QteDisponible = table.Column<int>(type: "int", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false),
                    Id_Produit = table.Column<int>(type: "int", nullable: false),
                    NumeroLot = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TypeStock = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    QteEchantillon = table.Column<int>(type: "int", nullable: true),
                    QteGratuite = table.Column<int>(type: "int", nullable: true),
                    TypePromotion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id_stock);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Distributions_Echantillons_Id_Medecin",
                table: "Distributions_Echantillons",
                column: "Id_Medecin");

            migrationBuilder.CreateIndex(
                name: "IX_Distributions_Echantillons_NumeroLot",
                table: "Distributions_Echantillons",
                column: "NumeroLot");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Id_User_Delegue",
                table: "Stocks",
                column: "Id_User_Delegue");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_NumeroLot",
                table: "Stocks",
                column: "NumeroLot");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Distributions_Echantillons");

            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
