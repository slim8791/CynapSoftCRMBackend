using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoLocationToRapport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add nullable GPS columns to the Rapports table.
            // Both are float (double-precision) and deliberately nullable so that
            // rapports submitted when GPS is unavailable or refused are still saved.
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Rapports",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Rapports",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Rapports");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Rapports");
        }
    }
}
