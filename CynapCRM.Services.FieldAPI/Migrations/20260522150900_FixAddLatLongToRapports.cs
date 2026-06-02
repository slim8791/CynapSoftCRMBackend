using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixAddLatLongToRapports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely add Latitude column if it doesn't already exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'Latitude'
                )
                BEGIN
                    ALTER TABLE [Rapports] ADD [Latitude] float NULL;
                END
            ");

            // Safely add Longitude column if it doesn't already exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'Longitude'
                )
                BEGIN
                    ALTER TABLE [Rapports] ADD [Longitude] float NULL;
                END
            ");

            // Safely add IdSuperviseurValidateur column if it doesn't already exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'Rapports') AND name = 'IdSuperviseurValidateur'
                )
                BEGIN
                    ALTER TABLE [Rapports] ADD [IdSuperviseurValidateur] int NULL;
                END
            ");
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

            migrationBuilder.DropColumn(
                name: "IdSuperviseurValidateur",
                table: "Rapports");
        }
    }
}
