using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeObjectifToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ 1️⃣ Conversion STRING → INT (selon enum TypeObjectif)
            migrationBuilder.Sql(@"
        UPDATE Objectifs SET Type = '1' WHERE Type = 'VISITES';
        UPDATE Objectifs SET Type = '2' WHERE Type = 'CHIFFREAFFAIRES';
        UPDATE Objectifs SET Type = '3' WHERE Type = 'NOUVEAUXCLIENTS';
        UPDATE Objectifs SET Type = '4' WHERE Type = 'FIDELISATION';
    ");

            migrationBuilder.Sql(@"
        UPDATE Objectifs SET Periode = '1' WHERE Periode = 'MENSUEL';
        UPDATE Objectifs SET Periode = '2' WHERE Periode = 'TRIMESTRIEL';
        UPDATE Objectifs SET Periode = '3' WHERE Periode = 'ANNUEL';
    ");

            // ✅ 2️⃣ Changement du type des colonnes
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Objectifs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Periode",
                table: "Objectifs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ✅ 3️⃣ Ajout des dates
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDebut",
                table: "Objectifs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFin",
                table: "Objectifs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DateDebut", table: "Objectifs");
            migrationBuilder.DropColumn(name: "DateFin", table: "Objectifs");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Objectifs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Periode",
                table: "Objectifs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.Sql(@"
        UPDATE Objectifs SET Type = 'VISITES' WHERE Type = '1';
        UPDATE Objectifs SET Type = 'CHIFFREAFFAIRES' WHERE Type = '2';
        UPDATE Objectifs SET Type = 'NOUVEAUXCLIENTS' WHERE Type = '3';
        UPDATE Objectifs SET Type = 'FIDELISATION' WHERE Type = '4';

        UPDATE Objectifs SET Periode = 'MENSUEL' WHERE Periode = '1';
        UPDATE Objectifs SET Periode = 'TRIMESTRIEL' WHERE Periode = '2';
        UPDATE Objectifs SET Periode = 'ANNUEL' WHERE Periode = '3';
    ");
        }

    }
}
