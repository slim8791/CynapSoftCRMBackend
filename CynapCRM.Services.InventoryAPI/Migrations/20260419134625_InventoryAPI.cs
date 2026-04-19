using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class InventoryAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id_User_Delegue",
                table: "Distributions_Echantillons",
                newName: "Id_Delegue");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Stocks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Stocks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateExpiration",
                table: "Stocks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Stocks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QteReservee",
                table: "Stocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Distributions_Echantillons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Id_Medecin",
                table: "Distributions_Echantillons",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDistribution",
                table: "Distributions_Echantillons",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "Id_Pharmacien",
                table: "Distributions_Echantillons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Distributions_Echantillons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Stock_Movements",
                columns: table => new
                {
                    Id_Movement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Stock = table.Column<int>(type: "int", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false),
                    TypeMovement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateMovement = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock_Movements", x => x.Id_Movement);
                    table.ForeignKey(
                        name: "FK_Stock_Movements_Stocks_Id_Stock",
                        column: x => x.Id_Stock,
                        principalTable: "Stocks",
                        principalColumn: "Id_stock",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stock_Movements_Id_Stock",
                table: "Stock_Movements",
                column: "Id_Stock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stock_Movements");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "DateExpiration",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "QteReservee",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Id_Pharmacien",
                table: "Distributions_Echantillons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Distributions_Echantillons");

            migrationBuilder.RenameColumn(
                name: "Id_Delegue",
                table: "Distributions_Echantillons",
                newName: "Id_User_Delegue");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Stocks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Distributions_Echantillons",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Id_Medecin",
                table: "Distributions_Echantillons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDistribution",
                table: "Distributions_Echantillons",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
