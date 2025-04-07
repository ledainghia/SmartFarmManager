using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFarmManager.DataAccessObject.Migrations
{
    /// <inheritdoc />
    public partial class fieldIsDeleteStandardPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StandardPrescriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StandardPrescriptions");
        }
    }
}
