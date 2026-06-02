using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddContactToPlanningVisite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_Medecin",
                table: "Plannings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_Pharmacien",
                table: "Plannings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeVisite",
                table: "Plannings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id_Medecin",
                table: "Plannings");

            migrationBuilder.DropColumn(
                name: "Id_Pharmacien",
                table: "Plannings");

            migrationBuilder.DropColumn(
                name: "TypeVisite",
                table: "Plannings");
        }
    }
}
