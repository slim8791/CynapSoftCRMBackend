using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Objectifs",
                columns: table => new
                {
                    Id_Objectif = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValeurCible = table.Column<int>(type: "int", nullable: false),
                    Periode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectifs", x => x.Id_Objectif);
                });

            migrationBuilder.CreateTable(
                name: "Plannings",
                columns: table => new
                {
                    Id_Planning = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeureDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeureFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Etat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plannings", x => x.Id_Planning);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id_Region = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodePostal = table.Column<int>(type: "int", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id_Region);
                });

            migrationBuilder.CreateTable(
                name: "Tournees",
                columns: table => new
                {
                    Id_Tournee = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_Planning = table.Column<int>(type: "int", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournees", x => x.Id_Tournee);
                    table.ForeignKey(
                        name: "FK_Tournees_Plannings_Id_Planning",
                        column: x => x.Id_Planning,
                        principalTable: "Plannings",
                        principalColumn: "Id_Planning",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visites",
                columns: table => new
                {
                    Id_Visite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false),
                    Id_Medecin = table.Column<int>(type: "int", nullable: true),
                    Id_Pharmacien = table.Column<int>(type: "int", nullable: true),
                    Id_Tournee = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visites", x => x.Id_Visite);
                    table.ForeignKey(
                        name: "FK_Visites_Tournees_Id_Tournee",
                        column: x => x.Id_Tournee,
                        principalTable: "Tournees",
                        principalColumn: "Id_Tournee",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rapports",
                columns: table => new
                {
                    Id_Rapport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Visite = table.Column<int>(type: "int", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rapports", x => x.Id_Rapport);
                    table.ForeignKey(
                        name: "FK_Rapports_Visites_Id_Visite",
                        column: x => x.Id_Visite,
                        principalTable: "Visites",
                        principalColumn: "Id_Visite",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Objectifs_Id_User_Delegue",
                table: "Objectifs",
                column: "Id_User_Delegue");

            migrationBuilder.CreateIndex(
                name: "IX_Plannings_Id_User_Delegue",
                table: "Plannings",
                column: "Id_User_Delegue");

            migrationBuilder.CreateIndex(
                name: "IX_Rapports_Id_User_Delegue",
                table: "Rapports",
                column: "Id_User_Delegue");

            migrationBuilder.CreateIndex(
                name: "IX_Rapports_Id_Visite",
                table: "Rapports",
                column: "Id_Visite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CodePostal",
                table: "Regions",
                column: "CodePostal");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Id_User_Delegue",
                table: "Regions",
                column: "Id_User_Delegue");

            migrationBuilder.CreateIndex(
                name: "IX_Tournees_Id_Planning",
                table: "Tournees",
                column: "Id_Planning");

            migrationBuilder.CreateIndex(
                name: "IX_Visites_Id_Tournee",
                table: "Visites",
                column: "Id_Tournee");

            migrationBuilder.CreateIndex(
                name: "IX_Visites_Id_User_Delegue",
                table: "Visites",
                column: "Id_User_Delegue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Objectifs");

            migrationBuilder.DropTable(
                name: "Rapports");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Visites");

            migrationBuilder.DropTable(
                name: "Tournees");

            migrationBuilder.DropTable(
                name: "Plannings");
        }
    }
}
