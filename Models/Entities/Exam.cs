namespace Quiz_App.Models.Entities
{
    public class Exam
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public int Duration { get; set; }
        public bool Randomize { get; set; }

        //1:N relationship with Questions
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }


}
