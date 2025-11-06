using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeSeeu.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderTotalPriceDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEAPnVjHaVrnwwvttH4wSY3j4o/rsIU80XE7a4vgsyAPV0nfYzPEOXu0lytnFv/aWgQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEEWNb0NW6F3YvJTwH0UqFVDaUXbk1lgF1Y47VKyM+E2lwJweaYv2vHFets9vwuah1A==");
        }
    }
}
