using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CynapCRM.Services.FieldAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVisiteStartFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HeureDebut",
                table: "Visites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarted",
                table: "Visites",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeureDebut",
                table: "Visites");

            migrationBuilder.DropColumn(
                name: "IsStarted",
                table: "Visites");
        }
    }
}
