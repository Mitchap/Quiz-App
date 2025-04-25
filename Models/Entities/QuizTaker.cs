using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz_App.Models.Entities
{
    [Table("QuizTaker")]
    public class QuizTaker
    {
        public Guid Id { get; set; }
        public Guid? ExamId { get; set; }
        public string? FName {  get; set; }
        public string? LName { get; set; }
        public string? Email { get; set; }
        public string? Pin { get; set; }
        public string? LastSchool { get; set; }
        public int? Score { get; set; }

        public virtual Exam? Exam { get; set; }
    }
}
