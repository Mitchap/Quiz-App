using Quiz_App.Models.Entities;
using Quiz_App.Models;

namespace Quiz_App.Models
{
    public class ExamViewModel
    {
        public class Exam
        {
            public required string Title { get; set; }
            public int Duration { get; set; }
            public bool Randomize { get; set; }

            //1:N relationship with Questions
            public ICollection<Question> Questions { get; set; } = new List<Question>();
        }

    }
}
