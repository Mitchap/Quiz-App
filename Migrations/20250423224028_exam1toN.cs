using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_App.Migrations
{
    /// <inheritdoc />
    public partial class exam1toN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExamId",
                table: "QuizTaker",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizTaker_ExamId",
                table: "QuizTaker",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizTaker_Exams_ExamId",
                table: "QuizTaker",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizTaker_Exams_ExamId",
                table: "QuizTaker");

            migrationBuilder.DropIndex(
                name: "IX_QuizTaker_ExamId",
                table: "QuizTaker");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "QuizTaker");
        }
    }
}
