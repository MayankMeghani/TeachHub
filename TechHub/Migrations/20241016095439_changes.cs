using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachHub.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Learners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Learners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
