using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Quiz_App.Data;
using Quiz_App.Models.Entities;
using System.Net.Mail;
using System.Net;
using static Org.BouncyCastle.Math.EC.ECCurve;


namespace Quiz_App.Controllers
{

    [Authorize]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly EmailService _emailService; // EmailService dependency
        private readonly IConfiguration _config;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ExamController(ApplicationDbContext context, EmailService emailService, IConfiguration config)
        {
            dbContext = context;
            _emailService = emailService;
            _config = config;
            _baseUrl = _config["AppSettings:BaseUrl"];
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

        public async Task<IActionResult> DuplicateExam(Guid id)
        {
            var toCopy = await dbContext.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (toCopy == null)
            {
                return NotFound();
            }

            var copy = new Exam
            {
                Id = Guid.NewGuid(), 
                Title = toCopy.Title + " (Copy)", 
                Duration = toCopy.Duration,
                Randomize = toCopy.Randomize,
                IsPublished = toCopy.IsPublished,
                TotalScore = toCopy.TotalScore,
                Questions = toCopy.Questions.Select(q => new Question
                {
                    Id = Guid.NewGuid(), 
                    ExamId = toCopy.Id, 
                    QuestionTitle = q.QuestionTitle,
                    Choice_1 = q.Choice_1,
                    Choice_2 = q.Choice_2,
                    Choice_3 = q.Choice_3,
                    Choice_4 = q.Choice_4,
                    CorrectAnswer = q.CorrectAnswer,
                    Score = q.Score
                }).ToList()
            };

            dbContext.Exams.Add(copy);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("ListExam");
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

            exam.Status = exam.Status == "Ongoing" ? "Pending" : "Ongoing";
            exam.IsPublished = !exam.IsPublished;

            if (exam.IsPublished)
            {
                var quizTakers = await dbContext.QuizTakers
                    .Where(qt => qt.ExamId == id && !qt.IsUsed)
                    .ToListAsync();
            
                foreach(var quizTaker in quizTakers)
                {
                    string email = quizTaker.Email;
                    string subject = $"You have been invited to take the quiz '{exam.Title}'";
                    string quizLink = $"{_baseUrl}/TakeQuiz?examId={exam.Id}";
                    string body = $"Hello, you have been invited to take the quiz '{exam.Title}'.<br><br>" +
                                  $"Use the following link to access the quiz: <a href='{quizLink}'>Take Quiz</a><br>" +
                                  $"Your PIN: {quizTaker.Pin}";

                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, body);

                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Failed to send email: {ex.Message}";
                    }
                }
            }

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

        public async Task<IActionResult> ExamPreview(Guid id)
        {
            var exam = await dbContext.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == id);

            return View("ExamPreview",exam);
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
            if (exam.IsPublished)
            {
                try
                {
                    var subject = $"Access to Quiz: {exam.Title}";
                    var quizLink = $"{_baseUrl}/TakeQuiz?examId={examId}";
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
            }
            
            return RedirectToAction("ExamDetails", new { id = examId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDone(Guid id)
        {
            var exam = await dbContext.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            exam.Status = "Done";
            await dbContext.SaveChangesAsync();

            return RedirectToAction("ExamDetails", new { id });
        }
    }
}
