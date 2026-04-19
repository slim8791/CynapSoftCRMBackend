using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class FieldAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visites_Tournees_Id_Tournee",
                table: "Visites");

            migrationBuilder.DropTable(
                name: "Tournees");

            migrationBuilder.DropIndex(
                name: "IX_Visites_Id_Tournee",
                table: "Visites");

            migrationBuilder.RenameColumn(
                name: "Id_Tournee",
                table: "Visites",
                newName: "Id_Region");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Visites",
                newName: "DateVisite");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Rapports",
                newName: "DateRapport");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Visites",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Id_Planning",
                table: "Visites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Visites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HeureFin",
                table: "Plannings",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HeureDebut",
                table: "Plannings",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Etat",
                table: "Plannings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Visites_Id_Planning",
                table: "Visites",
                column: "Id_Planning");

            migrationBuilder.AddForeignKey(
                name: "FK_Visites_Plannings_Id_Planning",
                table: "Visites",
                column: "Id_Planning",
                principalTable: "Plannings",
                principalColumn: "Id_Planning",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visites_Plannings_Id_Planning",
                table: "Visites");

            migrationBuilder.DropIndex(
                name: "IX_Visites_Id_Planning",
                table: "Visites");

            migrationBuilder.DropColumn(
                name: "Id_Planning",
                table: "Visites");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Visites");

            migrationBuilder.RenameColumn(
                name: "Id_Region",
                table: "Visites",
                newName: "Id_Tournee");

            migrationBuilder.RenameColumn(
                name: "DateVisite",
                table: "Visites",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "DateRapport",
                table: "Rapports",
                newName: "Date");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Visites",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HeureFin",
                table: "Plannings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HeureDebut",
                table: "Plannings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<string>(
                name: "Etat",
                table: "Plannings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Tournees",
                columns: table => new
                {
                    Id_Tournee = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Planning = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_User_Delegue = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Visites_Id_Tournee",
                table: "Visites",
                column: "Id_Tournee");

            migrationBuilder.CreateIndex(
                name: "IX_Tournees_Id_Planning",
                table: "Tournees",
                column: "Id_Planning");

            migrationBuilder.AddForeignKey(
                name: "FK_Visites_Tournees_Id_Tournee",
                table: "Visites",
                column: "Id_Tournee",
                principalTable: "Tournees",
                principalColumn: "Id_Tournee",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
