using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoStockFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDebut",
                table: "Stocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFin",
                table: "Stocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Stocks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteAchat",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteGratuite",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Stock_Gratuite_DateDebut",
                table: "Stocks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Stock_Gratuite_DateFin",
                table: "Stocks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDebut",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "DateFin",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "QuantiteAchat",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "QuantiteGratuite",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Stock_Gratuite_DateDebut",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Stock_Gratuite_DateFin",
                table: "Stocks");
        }
    }
}
