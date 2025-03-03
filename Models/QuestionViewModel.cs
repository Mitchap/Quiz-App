using Quiz_App.Models.Entities;
using Quiz_App.Models;


namespace Quiz_App.Models

{
    public class QuestionViewModel
    {
        public class Question
        {
            public Guid ExamId { get; set; }
            public required string QuestionTitle { get; set; }
            public required string Choice_1 { get; set; }
            public required string Choice_2 { get; set; }
            public required string Choice_3 { get; set; }
            public required string Choice_4 { get; set; }
            public string? CorrectAnswer { get; set; }

        }
    }
}
