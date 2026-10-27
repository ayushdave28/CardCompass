using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Services
{
    public interface IFeedbackService
    {
        Task SubmitFeedbackAsync(Feedback feedback);
        Task<List<Feedback>> GetAllFeedbacksAsync();
        Task MarkReviewedAsync(int id);
        Task<int> GetPendingCountAsync();
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitFeedbackAsync(Feedback feedback)
        {
            feedback.SubmittedAt = DateTime.UtcNow;
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Feedback>> GetAllFeedbacksAsync()
        {
            return await _context.Feedbacks
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task MarkReviewedAsync(int id)
        {
            var item = await _context.Feedbacks.FindAsync(id);
            if (item != null)
            {
                item.IsReviewed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.Feedbacks.CountAsync(f => !f.IsReviewed);
        }
    }
}
