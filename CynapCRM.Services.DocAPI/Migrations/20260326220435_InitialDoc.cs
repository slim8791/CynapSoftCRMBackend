using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.DocAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialDoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_Documents_Commerciaux",
                columns: table => new
                {
                    Numero_Doc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom_Doc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Commande = table.Column<int>(type: "int", nullable: false),
                    Id_Client = table.Column<int>(type: "int", nullable: true),
                    TypeDocument = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Id_BC = table.Column<int>(type: "int", nullable: true),
                    Id_BL = table.Column<int>(type: "int", nullable: true),
                    Id_Facture = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_Documents_Commerciaux", x => x.Numero_Doc);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_Documents_Commerciaux_Id_Client",
                table: "T_Documents_Commerciaux",
                column: "Id_Client");

            migrationBuilder.CreateIndex(
                name: "IX_T_Documents_Commerciaux_Id_Commande",
                table: "T_Documents_Commerciaux",
                column: "Id_Commande");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_Documents_Commerciaux");
        }
    }
}
