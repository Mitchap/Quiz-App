namespace Quiz_App.Models.Entities
{
    public class Exam
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public int Duration { get; set; }
        public bool Randomize { get; set; }
        public bool IsPublished { get; set; } = false;
        public int TotalScore { get; set; }
        public DateTime? PublishDateTime { get; set; }

        //1:N relationship with Questions
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }


}
