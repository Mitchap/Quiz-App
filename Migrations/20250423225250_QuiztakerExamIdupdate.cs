using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_App.Migrations
{
    /// <inheritdoc />
    public partial class QuiztakerExamIdupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "QuizTaker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuizId",
                table: "QuizTaker",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
