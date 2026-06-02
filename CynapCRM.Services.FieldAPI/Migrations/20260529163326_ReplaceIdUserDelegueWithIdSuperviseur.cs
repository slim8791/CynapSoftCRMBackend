using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIdUserDelegueWithIdSuperviseur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Regions_Id_User_Delegue",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Id_User_Delegue",
                table: "Regions");

            migrationBuilder.AddColumn<int>(
                name: "Id_Superviseur",
                table: "Regions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Id_Superviseur",
                table: "Regions",
                column: "Id_Superviseur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Regions_Id_Superviseur",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Id_Superviseur",
                table: "Regions");

            migrationBuilder.AddColumn<int>(
                name: "Id_User_Delegue",
                table: "Regions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Id_User_Delegue",
                table: "Regions",
                column: "Id_User_Delegue");
        }
    }
}
