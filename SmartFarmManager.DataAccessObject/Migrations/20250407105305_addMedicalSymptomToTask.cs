using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFarmManager.DataAccessObject.Migrations
{
    /// <inheritdoc />
    public partial class addMedicalSymptomToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MedicalSymptomId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalSymptomId",
                table: "Tasks");
        }
    }
}
