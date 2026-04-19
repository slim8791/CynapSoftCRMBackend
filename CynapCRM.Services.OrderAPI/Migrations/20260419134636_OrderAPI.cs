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
            migrationBuilder.AlterColumn<int>(
                name: "Statut",
                table: "Reclamations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Id_Ligne",
                table: "Reclamations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Remise",
                table: "LignesCommandes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaire",
                table: "LignesCommandes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Reclamations_Id_Ligne",
                table: "Reclamations",
                column: "Id_Ligne");

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamations_LignesCommandes_Id_Ligne",
                table: "Reclamations",
                column: "Id_Ligne",
                principalTable: "LignesCommandes",
                principalColumn: "Id_Ligne",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reclamations_LignesCommandes_Id_Ligne",
                table: "Reclamations");

            migrationBuilder.DropIndex(
                name: "IX_Reclamations_Id_Ligne",
                table: "Reclamations");

            migrationBuilder.DropColumn(
                name: "Id_Ligne",
                table: "Reclamations");

            migrationBuilder.DropColumn(
                name: "PrixUnitaire",
                table: "LignesCommandes");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Reclamations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Remise",
                table: "LignesCommandes",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
