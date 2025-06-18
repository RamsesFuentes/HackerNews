using HackerNews.Core.Models;
using HackerNews.Core.Repositories;
using HackerNews.Core.Services;

namespace HackerNews.Services
{
    public class StoryService(IStoryRepository storyRepository, ILogger<StoryService> logger) : IStoryService
    {
        private readonly IStoryRepository m_storyRepository = storyRepository;
        private readonly ILogger<StoryService> m_logger = logger;

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int quantity)
        {
            IEnumerable<int> storiesIds = await m_storyRepository.GetBestStoriesIdsAsync();
            IEnumerable<Task<Story?>> storiesTasks = storiesIds.Select(m_storyRepository.GetStoryByIdAsync);
            IEnumerable<Story?> nullableStories = await Task.WhenAll(storiesTasks);
            IEnumerable<Story> stories = nullableStories.OfType<Story>();
            if (stories.Count() < nullableStories.Count())
            {
                IEnumerable<int> storiesNotFoundIds = storiesIds.Except(stories.Select(story => story.Id));
                m_logger.LogWarning($"Stories not found: {string.Join(',', storiesNotFoundIds)}");
            }
            return stories.OrderByDescending(story => story.Score).Take(quantity);

        }
    }
}
