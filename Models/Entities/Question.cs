namespace Quiz_App.Models.Entities
{
    public class Question
    {
        public Guid Id { get; set; }

        //Foreign key for Exam that the question belongs to
        public required Guid ExamId { get; set; }

        public required string QuestionTitle {  get; set; }
        
        public required string Choice_1{ get; set; }
        public required string Choice_2{ get; set; }
        public required string Choice_3{ get; set; }
        public required string Choice_4{ get; set; }
        public string? CorrectAnswer { get; set; }
        public int Score { get; set; } = 1; //default score is 1

        //Navigation property to indicate N:1 relationship with Exam table
        public Exam? Exam { get; set; }
    }
}