using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eleganza.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryCityId",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliverySubCityId",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapUrl",
                table: "orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCityId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliverySubCityId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "MapUrl",
                table: "orders");
        }
    }
}
