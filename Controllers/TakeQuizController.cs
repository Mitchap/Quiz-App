using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Quiz_App.Data;
using Quiz_App.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Quiz_App.Models;

namespace Quiz_App.Controllers
{
    public class TakeQuizController : Controller
    {

        private readonly ApplicationDbContext _context;

        public TakeQuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPin(QuizTaker model)
        {
            if (ModelState.IsValid)
            {
                // Save the data to the database
                var quizTakerEntry = new QuizTaker
                {
                    Pin = model.Pin,
                };

                await _context.QuizTakers.AddAsync(quizTakerEntry);
                await _context.SaveChangesAsync();

                TempData["QuizTakerId"] = quizTakerEntry.Id;

                return RedirectToAction("Instructions");
            }
            return View();

        }

        public IActionResult Instructions()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDetails(QuizTaker model)
        {
            if (ModelState.IsValid)
            {
                var existingQuizTaker = await _context.QuizTakers.FindAsync(model.Id);
                if (existingQuizTaker != null)
                {
                    existingQuizTaker.FName = model.FName;
                    existingQuizTaker.LName = model.LName;
                    existingQuizTaker.LastSchool = model.LastSchool;
                    existingQuizTaker.ExamId = model.ExamId;

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Quiz", new { id = existingQuizTaker.Id });
                }
            }

            return View("Instructions", model);
        }
        public IActionResult Quiz(Guid id)
        {
            var quizTaker = _context.QuizTakers
                .Include(qt => qt.Exam)       // Load the associated Exam
                .ThenInclude(ex => ex.Questions)  // Load the associated Questions of the Exam
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

                string userAnswer = Answers[i]; // e.g., "choice1"
                string? userAnswerValue = userAnswer switch
                {
                    "choice1" => question.Choice_1,
                    "choice2" => question.Choice_2,
                    "choice3" => question.Choice_3,
                    "choice4" => question.Choice_4,
                    _ => null
                };

                string? correctAnswerValue = question.CorrectAnswer switch
                {
                    "choice1" => question.Choice_1,
                    "choice2" => question.Choice_2,
                    "choice3" => question.Choice_3,
                    "choice4" => question.Choice_4,
                    _ => question.CorrectAnswer // fallback if it's the actual value
                };

                bool isCorrect = userAnswerValue?.Trim().ToLower() == correctAnswerValue?.Trim().ToLower();
                if (isCorrect) score+=2;

                results.Add(new QuestionResult
                {
                    Question = question,
                    UserAnswer = userAnswer, // Still "choice1", etc.
                    IsCorrect = isCorrect
                });
            }

            // Save the score
            quizTaker.Score = score;
            _context.SaveChanges();

            // Prepare the result model
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
