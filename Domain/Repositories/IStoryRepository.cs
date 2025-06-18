using HackerNews.Core.Models;

namespace HackerNews.Core.Repositories
{
    public interface IStoryRepository
    {
        public Task<IEnumerable<int>> GetBestStoriesIdsAsync();
        public Task<Story?> GetStoryByIdAsync(int storyId);
    }
}
