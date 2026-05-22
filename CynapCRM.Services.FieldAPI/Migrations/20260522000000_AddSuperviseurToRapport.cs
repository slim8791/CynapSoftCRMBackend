using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperviseurToRapport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdSuperviseurValidateur",
                table: "Rapports",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdSuperviseurValidateur",
                table: "Rapports");
        }
    }
}
