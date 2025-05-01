using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_App.Data;
using Quiz_App.Models.Entities;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using Quiz_App.Models;

namespace Quiz_App.Controllers
{
    public class TakeQuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TakeQuizController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddQuizTaker(string email, Guid examId)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ModelState.AddModelError("", "Invalid email format.");
                return View("Index");
            }

            var newId = Guid.NewGuid();
            var pin = newId.ToString().Substring(0, 6);

            var quizTaker = new QuizTaker
            {
                Id = newId,
                Email = email,
                ExamId = examId,
                Pin = pin,
            };

            await _context.QuizTakers.AddAsync(quizTaker);
            await _context.SaveChangesAsync();

            var exam = await _context.Exams.FindAsync(examId);
            if (exam != null)
            {
                try
                {
                    // Sending Email
                    var emailSent = await SendEmailAsync(email, exam, pin, examId);
                    if (!emailSent)
                    {
                        ModelState.AddModelError("", "Failed to send email. Please try again.");
                        return View("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error sending email: {ex.Message}");
                    return View("Index");
                }
            }

            return RedirectToAction("ExamDetails", "Exam", new { id = examId });
        }

        private async Task<bool> SendEmailAsync(string email, Exam exam, string pin, Guid examId)
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:Port"]),
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Username"],
                        _configuration["EmailSettings:Password"]
                    ),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(
                        _configuration["EmailSettings:SenderEmail"],
                        _configuration["EmailSettings:SenderName"]
                    ),
                    Subject = $"Access to Quiz: {exam.Title}",
                    Body = $"You have been invited to take the quiz '{exam.Title}'.\n\nUse this link to access the quiz: https://yourapp.com/TakeQuiz?examId={examId}\nYour PIN: {pin}",
                    IsBodyHtml = false
                };
                message.To.Add(email);

                await smtpClient.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email: {ex.Message}");
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPin(string email, string pin)
        {
            var quizTaker = await _context.QuizTakers
                .FirstOrDefaultAsync(q => q.Email == email && q.Pin == pin);

            if (quizTaker == null)
            {
                ModelState.AddModelError("", "Invalid email or PIN.");
                return View("Index");
            }

            else if (quizTaker.IsUsed)
            {
                ModelState.AddModelError("", "The PIN is already used");
                return View("Index");
            }

            quizTaker.IsUsed = true;
            await _context.SaveChangesAsync();

            TempData["QuizTakerId"] = quizTaker.Id;
            return RedirectToAction("Instructions");
        }

        public async Task<IActionResult> Instructions()
        {
            if (TempData["QuizTakerId"] is Guid quizTakerId)
            {
                var quizTaker = await _context.QuizTakers.FindAsync(quizTakerId);
                if (quizTaker != null)
                {
                    // Rehydrate Email and Pin into the model
                    if (TempData["Email"] is string email)
                        quizTaker.Email = email;
                    if (TempData["Pin"] is string pin)
                        quizTaker.Pin = pin;

                    return View(quizTaker);
                }
            }

            return RedirectToAction("Index"); // fallback
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDetails(QuizTaker model)
        {
            Console.WriteLine($"SubmitDetails hit. Id: {model.Id}, FName: {model.FName}");

            if (ModelState.IsValid)
            {
                var existingQuizTaker = await _context.QuizTakers.FindAsync(model.Id);
                if (existingQuizTaker != null)
                {
                    // Make sure ExamId is not null
                    if (model.ExamId.HasValue)
                    {
                        existingQuizTaker.FName = model.FName;
                        existingQuizTaker.LName = model.LName;
                        existingQuizTaker.LastSchool = model.LastSchool;
                        existingQuizTaker.ExamId = model.ExamId;
                        existingQuizTaker.Status = "Ongoing";

                        await _context.SaveChangesAsync();

                        return RedirectToAction("Quiz", new { id = existingQuizTaker.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "ExamId is missing.");
                    }
                }
            }

            // If we reached here, there were model state errors or something went wrong, show the form again with errors
            return View("Instructions", model);
        }


        public IActionResult Quiz(Guid id)
        {
            var quizTaker = _context.QuizTakers
                .Include(qt => qt.Exam)
                .ThenInclude(ex => ex.Questions)
                .FirstOrDefault(qt => qt.Id == id);

            if (quizTaker == null || quizTaker.Exam == null)
            {
                return NotFound();
            }

            return View(quizTaker);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(Guid QuizTakerId, List<Guid> QuestionIds, List<string> Answers)
        {
            var quizTaker = _context.QuizTakers
                .Include(qt => qt.Exam)
                .ThenInclude(ex => ex.Questions)
                .FirstOrDefault(qt => qt.Id == QuizTakerId);

            if (quizTaker == null || quizTaker.Exam == null)
                return NotFound();

            var questions = quizTaker.Exam.Questions.ToList();
            var results = new List<QuestionResult>();
            int score = 0;

            for (int i = 0; i < QuestionIds.Count; i++)
            {
                var question = questions.FirstOrDefault(q => q.Id == QuestionIds[i]);
                if (question == null) continue;

                string userAnswer = Answers[i];

                string? userAnswerValue = userAnswer switch
                {
                    "Choice_1" => question.Choice_1,
                    "Choice_2" => question.Choice_2,
                    "Choice_3" => question.Choice_3,
                    "Choice_4" => question.Choice_4,
                    _ => userAnswer
                };

                string? correctAnswerValue = question.CorrectAnswer switch
                {
                    "Choice_1" => question.Choice_1,
                    "Choice_2" => question.Choice_2,
                    "Choice_3" => question.Choice_3,
                    "Choice_4" => question.Choice_4,
                    _ => question.CorrectAnswer
                };

                string normalizedUser = (userAnswerValue ?? "").Trim().ToLowerInvariant();
                string normalizedCorrect = (correctAnswerValue ?? "").Trim().ToLowerInvariant();

                bool isCorrect = normalizedUser == normalizedCorrect;

                if (isCorrect)
                    score += question.Score;

                results.Add(new QuestionResult
                {
                    Question = question,
                    UserAnswer = userAnswer,
                    IsCorrect = isCorrect
                });
            }

            quizTaker.Status = "Done";
            quizTaker.Score = score;
            _context.SaveChanges();

            var resultModel = new QuizResultViewModel
            {
                QuizTaker = quizTaker,
                Results = results,
                TotalScore = score,
                TotalQuestions = questions.Count
            };

            return View("QuizResult", resultModel);
        }
    }
}
