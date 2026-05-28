using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCodePostalToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CodePostal",
                table: "Regions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CodePostal",
                table: "Regions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
