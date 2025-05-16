using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_App.Migrations
{
    /// <inheritdoc />
    public partial class EmailMessageForExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailMessage",
                table: "Exams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailMessage",
                table: "Exams");
        }
    }
}
