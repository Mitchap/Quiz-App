using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Quiz_App.Data;
using Quiz_App.Models.Entities;


namespace Quiz_App.Controllers
{

    [Authorize]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly EmailService _emailService; // EmailService dependency


        // Constructor with EmailService injection
        public ExamController(ApplicationDbContext context, EmailService emailService)
        {
            dbContext = context;
            _emailService = emailService; // Initialize EmailService
        }

        //adding exams

        [HttpGet]
        public IActionResult AddExam()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddExam(Exam viewModel)
        {
            var exam = new Exam
            {
                Title = viewModel.Title,
                Duration = viewModel.Duration,
                Randomize = viewModel.Randomize,
                PublishDateTime = viewModel.PublishDateTime
            };
            
            await dbContext.Exams.AddAsync(exam);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("AddQuestion", "Question", new { examId = exam.Id });
        }

        //Listing exam in cards

        [HttpGet]
        public async Task<IActionResult> ListExam()
        {
            var exam = await dbContext.Exams.ToListAsync();

            return View(exam);
        }


        //Exam details
        //this will have access to exam details, and related questions


        [HttpGet]
        public async Task<IActionResult> ExamDetails(Guid id)
        {
            var exam = await dbContext.Exams
                .Include(e => e.Questions)
                .Include(e => e.QuizTakers) // include quiztakers in examdetails view
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        //Edit exams
        [HttpGet]

        public async Task<IActionResult> EditExam(Guid id)
        {
            var exam = await dbContext.Exams.FindAsync(id);

            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> EditExam(Exam viewModel)
        {

            var exam = await dbContext.Exams.FindAsync(viewModel.Id);

            if (exam is not null)
            {
                exam.Title = viewModel.Title;
                exam.Duration = viewModel.Duration;
                exam.Randomize = viewModel.Randomize;
                exam.PublishDateTime = viewModel.PublishDateTime;

                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("ListExam", "Exam");
        }
        public async Task<IActionResult> TogglePublish(Guid id)
        {
            var exam = await dbContext.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            exam.IsPublished = !exam.IsPublished;
            await dbContext.SaveChangesAsync();

            return RedirectToAction("ExamDetails", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExam(Guid id)
        {
            var exam = await dbContext.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null) return NotFound();

            dbContext.Questions.RemoveRange(exam.Questions);
            dbContext.Exams.Remove(exam);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("ListExam");
        }

        [HttpPost]
        public async Task<IActionResult> AddQuizTaker(Guid examId, string email)
        {
            var exam = await dbContext.Exams.FindAsync(examId);
            if (exam == null) return NotFound();

            // Create the QuizTaker
            var id = Guid.NewGuid();
            var pin = id.ToString("N").Substring(0, 6); // Take first 6 hex characters

            var quizTaker = new QuizTaker
            {
                Id = id,
                ExamId = examId,
                Email = email,
                Pin = pin
            };

            dbContext.QuizTakers.Add(quizTaker);
            await dbContext.SaveChangesAsync();

            // Send email with the PIN and exam details
            try
            {
                var subject = $"Access to Quiz: {exam.Title}";
                var quizLink = $"https://localhost:7166/TakeQuiz?examId={examId}";
                var body = $"You have been invited to take the quiz '{exam.Title}'.<br><br>" +
                           $"Use this link to access the quiz: <a href='{quizLink}'>Take Quiz</a><br>" +
                           $"Your PIN: {pin}";

                await _emailService.SendEmailAsync(email, subject, body);

                TempData["Success"] = $"Student invited with PIN: {pin}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to send email: {ex.Message}";
            }

            return RedirectToAction("ExamDetails", new { id = examId });
        }

    }
}
