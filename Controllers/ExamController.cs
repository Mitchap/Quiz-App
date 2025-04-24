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

        public ExamController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
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


    }
}
