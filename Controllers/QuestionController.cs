
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


        [HttpGet]
        public IActionResult AddQuestion(Guid examId)
        {
            var viewModel = new QuestionViewModel.Question
            {
                ExamId = examId,
                QuestionTitle = string.Empty, // Initialize required fields to avoid errors
                Choice_1 = string.Empty,
                Choice_2 = string.Empty,
                Choice_3 = string.Empty,
                Choice_4 = string.Empty,
                CorrectAnswer = string.Empty
            };
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> AddQuestion(Question viewModel)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(), // Ensure unique ID
                ExamId = viewModel.ExamId, // Correctly link to the exam
                QuestionTitle = viewModel.QuestionTitle,
                Choice_1 = viewModel.Choice_1,
                Choice_2 = viewModel.Choice_2,
                Choice_3 = viewModel.Choice_3,
                Choice_4 = viewModel.Choice_4,
                CorrectAnswer = viewModel.CorrectAnswer
            };

            await dbContext.Questions.AddAsync(question); 
            await dbContext.SaveChangesAsync();

            return RedirectToAction("ExamDetails", "Exam", new { id = viewModel.ExamId });
        }




    }
}
