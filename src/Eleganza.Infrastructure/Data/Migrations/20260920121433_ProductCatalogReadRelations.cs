using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eleganza.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductCatalogReadRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_products_vendors_VendorId",
                table: "products",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_vendors_VendorId",
                table: "products");
        }
    }
}
