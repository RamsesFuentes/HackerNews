using HackerNews.Core.Models;

namespace HackerNews.Core.Repositories
{
    public interface IStoryRepository
    {
        public Task<IEnumerable<int>> GetBestStoriesIdsAsync(int quantity);
        public Task<Story?> GetStoryByIdAsync(int storyId);
    }
}
