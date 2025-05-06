using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_App.Migrations
{
    /// <inheritdoc />
    public partial class addedStatusForQuizTaker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "QuizTaker",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "QuizTaker");
        }
    }
}
