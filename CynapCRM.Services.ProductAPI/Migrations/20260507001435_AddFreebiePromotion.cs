using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFreebiePromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Lots_NumeroLot",
                table: "Promotions");

            migrationBuilder.AlterColumn<float>(
                name: "Pourcentage",
                table: "Promotions",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Promotions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CodePromo",
                table: "Promotions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

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

            migrationBuilder.AddColumn<int>(
                name: "TypePromotion",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Lots_NumeroLot",
                table: "Promotions",
                column: "NumeroLot",
                principalTable: "Lots",
                principalColumn: "Numero",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Lots_NumeroLot",
                table: "Promotions");

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

            migrationBuilder.DropColumn(
                name: "TypePromotion",
                table: "Promotions");

            migrationBuilder.AlterColumn<float>(
                name: "Pourcentage",
                table: "Promotions",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLot",
                table: "Promotions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodePromo",
                table: "Promotions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Lots_NumeroLot",
                table: "Promotions",
                column: "NumeroLot",
                principalTable: "Lots",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
