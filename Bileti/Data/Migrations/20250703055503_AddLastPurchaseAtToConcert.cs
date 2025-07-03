using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bileti.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastPurchaseAtToConcert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPurchaseAt",
                table: "Concerts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPurchaseAt",
                table: "Concerts");
        }
    }
}
