using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_App.Data;
using Quiz_App.Models;
using Quiz_App.Models.Entities;

namespace Quiz_App.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public QuestionController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private void UpdateTotalScore(Guid examId)
        {
            var exam = dbContext.Exams
                .Include(e => e.Questions)
                .FirstOrDefault(e => e.Id == examId);

            if (exam != null)
            {
                exam.TotalScore = exam.Questions.Sum(q => q.Score);
                dbContext.SaveChanges();
            }
        }

        [HttpGet]
        public IActionResult AddQuestion(Guid examId)
        {
            var viewModel = new QuestionViewModel.Question
            {
                ExamId = examId,
                QuestionTitle = string.Empty,
                Choice_1 = string.Empty,
                Choice_2 = string.Empty,
                Choice_3 = string.Empty,
                Choice_4 = string.Empty,
                CorrectAnswer = null
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(Question viewModel)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                ExamId = viewModel.ExamId,
                QuestionTitle = viewModel.QuestionTitle,
                Choice_1 = viewModel.Choice_1,
                Choice_2 = viewModel.Choice_2,
                Choice_3 = viewModel.Choice_3,
                Choice_4 = viewModel.Choice_4,
                CorrectAnswer = viewModel.CorrectAnswer,
                Score = viewModel.Score
            };

            await dbContext.Questions.AddAsync(question);
            await dbContext.SaveChangesAsync();

            UpdateTotalScore(question.ExamId);

            return RedirectToAction("ExamDetails", "Exam", new { id = question.ExamId });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(Guid id)
        {
            var question = await dbContext.Questions.FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(Question updatedQuestion)
        {
            var question = await dbContext.Questions.FirstOrDefaultAsync(q => q.Id == updatedQuestion.Id);

            if (question == null)
            {
                return NotFound();
            }

            question.QuestionTitle = updatedQuestion.QuestionTitle;
            question.Choice_1 = updatedQuestion.Choice_1;
            question.Choice_2 = updatedQuestion.Choice_2;
            question.Choice_3 = updatedQuestion.Choice_3;
            question.Choice_4 = updatedQuestion.Choice_4;
            question.CorrectAnswer = updatedQuestion.CorrectAnswer;
            question.Score = updatedQuestion.Score;

            await dbContext.SaveChangesAsync();

            UpdateTotalScore(question.ExamId);

            return RedirectToAction("ExamDetails", "Exam", new { id = question.ExamId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await dbContext.Questions.FindAsync(id);

            if (question == null)
                return NotFound();

            var examId = question.ExamId;

            dbContext.Questions.Remove(question);
            await dbContext.SaveChangesAsync();

            UpdateTotalScore(examId);

            return RedirectToAction("ExamDetails", "Exam", new { id = examId });
        }
    }
}
