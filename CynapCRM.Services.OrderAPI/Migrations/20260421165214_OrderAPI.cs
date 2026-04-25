using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.OrderAPI.Migrations
{
    /// <inheritdoc />
    public partial class OrderAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    Id_Commande = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCommande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantTotalHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    Id_Client = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.Id_Commande);
                });

            migrationBuilder.CreateTable(
                name: "LignesCommandes",
                columns: table => new
                {
                    Id_Ligne = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    Remise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Commande = table.Column<int>(type: "int", nullable: false),
                    Id_Produit = table.Column<int>(type: "int", nullable: false),
                    NumeroLot = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesCommandes", x => x.Id_Ligne);
                    table.ForeignKey(
                        name: "FK_LignesCommandes_Commandes_Id_Commande",
                        column: x => x.Id_Commande,
                        principalTable: "Commandes",
                        principalColumn: "Id_Commande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reclamations",
                columns: table => new
                {
                    Id_Rec = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateReclamation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    Id_Commande = table.Column<int>(type: "int", nullable: false),
                    Id_Ligne = table.Column<int>(type: "int", nullable: false),
                    Id_Client = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reclamations", x => x.Id_Rec);
                    table.ForeignKey(
                        name: "FK_Reclamations_Commandes_Id_Commande",
                        column: x => x.Id_Commande,
                        principalTable: "Commandes",
                        principalColumn: "Id_Commande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reclamations_LignesCommandes_Id_Ligne",
                        column: x => x.Id_Ligne,
                        principalTable: "LignesCommandes",
                        principalColumn: "Id_Ligne");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_Id_Client",
                table: "Commandes",
                column: "Id_Client");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommandes_Id_Commande",
                table: "LignesCommandes",
                column: "Id_Commande");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommandes_Id_Produit",
                table: "LignesCommandes",
                column: "Id_Produit");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommandes_NumeroLot",
                table: "LignesCommandes",
                column: "NumeroLot");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_Id_Commande",
                table: "Reclamations",
                column: "Id_Commande");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_Id_Ligne",
                table: "Reclamations",
                column: "Id_Ligne");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reclamations");

            migrationBuilder.DropTable(
                name: "LignesCommandes");

            migrationBuilder.DropTable(
                name: "Commandes");
        }
    }
}
