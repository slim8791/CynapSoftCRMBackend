using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class NVVPPROD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id_Produit",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "PorteeSurTousLesLots",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "QuantiteGratuite",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "SeuilAchat",
                table: "Promotions");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "Lots",
                newName: "NumeroLot");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDebut",
                table: "Promotions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumeroLot",
                table: "Lots",
                newName: "Numero");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDebut",
                table: "Promotions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_Produit",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PorteeSurTousLesLots",
                table: "Promotions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteGratuite",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeuilAchat",
                table: "Promotions",
                type: "int",
                nullable: true);
        }
    }
}
