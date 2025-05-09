using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Quiz_App.Data;

public class ExamPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;

    public ExamPublisherService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;

            var examsToPublish = await dbContext.Exams
                .Where(e => e.PublishDateTime <= now && e.IsPublished == false)
                .ToListAsync();

            Console.WriteLine($"[{DateTime.Now}] ExamPublisherService checking for exams to publish...");

            if (examsToPublish.Any())
            {
                Console.WriteLine($"Found {examsToPublish.Count} exams to publish.");
            }

            foreach (var exam in examsToPublish)
            {
                exam.IsPublished = true;
            }

            if (examsToPublish.Any())
            {
                await dbContext.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

    }
}
