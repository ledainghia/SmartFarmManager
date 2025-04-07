using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFarmManager.DataAccessObject.Migrations
{
    /// <inheritdoc />
    public partial class addFieldForStatusLogAndVaccine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDose",
                table: "Vaccines",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalDose",
                table: "Vaccines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Log",
                table: "StatusLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerDose",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "TotalDose",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Log",
                table: "StatusLogs");
        }
    }
}
