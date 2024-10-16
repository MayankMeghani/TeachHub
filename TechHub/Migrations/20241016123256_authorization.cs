using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachHub.Migrations
{
    /// <inheritdoc />
    public partial class authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_AspNetUsers_Id",
                table: "Learners");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_Id",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Teachers",
                newName: "TeacherId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Learners",
                newName: "LearnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_AspNetUsers_LearnerId",
                table: "Learners",
                column: "LearnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_TeacherId",
                table: "Teachers",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_AspNetUsers_LearnerId",
                table: "Learners");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_TeacherId",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Teachers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LearnerId",
                table: "Learners",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_AspNetUsers_Id",
                table: "Learners",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_Id",
                table: "Teachers",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
