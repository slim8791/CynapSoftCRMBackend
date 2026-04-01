using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Id_Produit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrixVente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Prix_Creation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TVA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.Id_Produit);
                });

            migrationBuilder.CreateTable(
                name: "Lots",
                columns: table => new
                {
                    Numero = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    Id_Produit = table.Column<int>(type: "int", nullable: false),
                    Id_Promo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Numero);
                    table.ForeignKey(
                        name: "FK_Lots_Produits_Id_Produit",
                        column: x => x.Id_Produit,
                        principalTable: "Produits",
                        principalColumn: "Id_Produit",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Support_Markettings",
                columns: table => new
                {
                    Id_SupportMarketting = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_Produit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Support_Markettings", x => x.Id_SupportMarketting);
                    table.ForeignKey(
                        name: "FK_Support_Markettings_Produits_Id_Produit",
                        column: x => x.Id_Produit,
                        principalTable: "Produits",
                        principalColumn: "Id_Produit",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id_Promo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodePromo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Pourcentage = table.Column<float>(type: "real", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstActive = table.Column<bool>(type: "bit", nullable: false),
                    NumeroLot = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id_Promo);
                    table.ForeignKey(
                        name: "FK_Promotions_Lots_NumeroLot",
                        column: x => x.NumeroLot,
                        principalTable: "Lots",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fichiers",
                columns: table => new
                {
                    Id_Fichier = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Taille = table.Column<long>(type: "bigint", nullable: false),
                    Id_Support = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fichiers", x => x.Id_Fichier);
                    table.ForeignKey(
                        name: "FK_Fichiers_Support_Markettings_Id_Support",
                        column: x => x.Id_Support,
                        principalTable: "Support_Markettings",
                        principalColumn: "Id_SupportMarketting",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fichiers_Id_Support",
                table: "Fichiers",
                column: "Id_Support");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_Id_Produit",
                table: "Lots",
                column: "Id_Produit");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_NumeroLot",
                table: "Promotions",
                column: "NumeroLot");

            migrationBuilder.CreateIndex(
                name: "IX_Support_Markettings_Id_Produit",
                table: "Support_Markettings",
                column: "Id_Produit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fichiers");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Support_Markettings");

            migrationBuilder.DropTable(
                name: "Lots");

            migrationBuilder.DropTable(
                name: "Produits");
        }
    }
}
