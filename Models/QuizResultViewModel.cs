using Quiz_App.Models.Entities;

namespace Quiz_App.Models
{
    public class QuizResultViewModel
    {
        public QuizTaker QuizTaker { get; set; }
        public List<QuestionResult> Results { get; set; } = new();
        public int TotalScore { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class QuestionResult
    {
        public Question Question { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
