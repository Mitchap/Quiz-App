using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_App.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsUsedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "QuizTaker",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "QuizTaker");
        }
    }
}
