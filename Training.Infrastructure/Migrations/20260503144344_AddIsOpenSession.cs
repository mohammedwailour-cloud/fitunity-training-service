using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Training.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOpenSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpenSession",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpenSession",
                table: "Sessions");
        }
    }
}
