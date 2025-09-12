using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class employeedtooo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MealCost",
                table: "ExpenseHistories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherCost",
                table: "ExpenseHistories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportCost",
                table: "ExpenseHistories",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealCost",
                table: "ExpenseHistories");

            migrationBuilder.DropColumn(
                name: "OtherCost",
                table: "ExpenseHistories");

            migrationBuilder.DropColumn(
                name: "TransportCost",
                table: "ExpenseHistories");
        }
    }
}
