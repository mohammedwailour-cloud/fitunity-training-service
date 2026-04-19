using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Training.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalSpaceAndEventUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Events",
                newName: "DateFin");

            migrationBuilder.AddColumn<Guid>(
                name: "SpaceId",
                table: "Sessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDebut",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "SpaceId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Spaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    SupportsSeatManagement = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spaces", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SpaceId",
                table: "Sessions",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SpaceId",
                table: "Events",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaces_Code",
                table: "Spaces",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Spaces_SpaceId",
                table: "Events",
                column: "SpaceId",
                principalTable: "Spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Spaces_SpaceId",
                table: "Sessions",
                column: "SpaceId",
                principalTable: "Spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Spaces_SpaceId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Spaces_SpaceId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Spaces");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SpaceId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Events_SpaceId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SpaceId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "DateDebut",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SpaceId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "DateFin",
                table: "Events",
                newName: "Date");
        }
    }
}
