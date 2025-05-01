using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quiz_App.Models.Entities
{
    [Table("QuizTaker")]
    public class QuizTaker
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid? ExamId { get; set; }
        [Required]
        [StringLength(50)]
        public string? FName { get; set; }
        [Required]
        [StringLength(50)]
        public string? LName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN must be exactly 6 characters.")]
        public string? Pin { get; set; }
        [Required]
        [StringLength(100)]
        public string? LastSchool { get; set; }

        public int? Score { get; set; }
        public bool IsUsed { get; set; }
        public string? Status { get; set; }

        public virtual Exam? Exam { get; set; }
        public QuizTaker()
        {
            IsUsed = false;
            FName = "";
            LName = "";
            LastSchool = "";
            Status = "Pending";
        }
    }

}
