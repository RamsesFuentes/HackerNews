using HackerNews.Core.Models;

namespace HackerNews.Core.Services
{
    public interface IStoryService
    {
        public Task<IEnumerable<Story>> GetBestStoriesAsync(int quantity);
    }
}
