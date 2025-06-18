using Microsoft.Extensions.Caching.Memory;
using HackerNews.Core.Models;
using HackerNews.Core.Repositories;

namespace HackerNews.Persistence
{
    public class StoryRepository : IStoryRepository
    {
        private static SemaphoreSlim GetBestStoriesSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim GetStorySemaphore = new SemaphoreSlim(1, 1);
        private static TimeSpan SemaphoreTimeOut = TimeSpan.FromSeconds(30);

        private readonly HttpClient m_httpClient;
        private readonly IMemoryCache m_cache;
        private readonly ILogger<StoryRepository> m_logger;
        private readonly TimeSpan m_cacheDuration = TimeSpan.FromMinutes(5);
        private readonly string m_bestStoriesUrl = "beststories";
        private readonly string m_bestStoriesCacheKey = "best-stories-ids";
        private readonly string m_storyCacheKeyPrefix = "story";
        private readonly string m_hackerNewsBaseUrl = "https://hacker-news.firebaseio.com";
        private readonly string m_hackerNewsApiVersion = "v0";
        private readonly string m_hackerNewsItemsUrl = "item";
        private readonly string m_jsonExtension = ".json";

        public StoryRepository(HttpClient httpClient, IMemoryCache cache, ILogger<StoryRepository> logger)
        {
            m_httpClient = httpClient;
            m_httpClient.BaseAddress = new Uri($"{m_hackerNewsBaseUrl}/{m_hackerNewsApiVersion}/");
            m_cache = cache;
            m_logger = logger;
        }

        public async Task<IEnumerable<int>> GetBestStoriesIdsAsync(int quantity)
        {
            bool acquired = false;
            try
            {
                if (quantity <= 0) return Enumerable.Empty<int>();

                if (m_cache.TryGetValue(m_bestStoriesCacheKey, out IEnumerable<int>? storiesIds)) return (storiesIds ?? Enumerable.Empty<int>()).Take(quantity);

                acquired = await GetBestStoriesSemaphore.WaitAsync(SemaphoreTimeOut);
                if (!acquired)
                {
                    m_logger.LogError("Failed to acquire mutex for GetBestStoriesIdsAsync");
                    return Enumerable.Empty<int>();
                }

                if (m_cache.TryGetValue(m_bestStoriesCacheKey, out storiesIds))
                { 
                    GetBestStoriesSemaphore.Release();
                    return (storiesIds ?? Enumerable.Empty<int>()).Take(quantity);
                }

                storiesIds = await m_httpClient.GetFromJsonAsync<List<int>>($"{m_bestStoriesUrl}{m_jsonExtension}");
                if(storiesIds is null)
                {
                    GetBestStoriesSemaphore.Release();
                    m_logger.LogWarning("Best stories ids not found");
                    return Enumerable.Empty<int>();
                }
                
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(m_cacheDuration);
                m_cache.Set(m_bestStoriesCacheKey, storiesIds, cacheEntryOptions);

                GetBestStoriesSemaphore.Release();
                return storiesIds.Take(quantity);
            }
            catch (Exception e)
            {
                if (acquired) GetBestStoriesSemaphore.Release();
                m_logger.LogError(e, "Error fetching best stories");
                return Enumerable.Empty<int>();
            }
        }

        public async Task<Story?> GetStoryByIdAsync(int storyId)
        {
            bool acquired = false;
            try
            {
                if (storyId <= 0) return null;

                string storyCacheKey = $"{m_storyCacheKeyPrefix}-{storyId}";

                if (m_cache.TryGetValue(storyCacheKey, out Story? story)) return story;

                acquired = await GetStorySemaphore.WaitAsync(SemaphoreTimeOut);
                if (!acquired)
                {
                    m_logger.LogError("Failed to acquire mutex for GetStoryByIdAsync");
                    return null;
                }

                if (m_cache.TryGetValue(storyCacheKey, out story))
                {
                    GetStorySemaphore.Release();
                    return story;
                }

                string url = $"{m_hackerNewsItemsUrl}/{storyId}{m_jsonExtension}";
                ApiStory? apiStory = await m_httpClient.GetFromJsonAsync<ApiStory>(url);
                if (apiStory is null)
                {
                    GetStorySemaphore.Release();
                    m_logger.LogWarning($"Story not found: {storyId}");
                    return null;
                }

                story = new Story
                {
                    Id = storyId,
                    Title = apiStory.Title ?? string.Empty,
                    Uri = apiStory.Url ?? string.Empty,
                    PostedBy = apiStory.By ?? string.Empty,
                    Time = DateTimeOffset.FromUnixTimeSeconds(apiStory.Time).UtcDateTime,
                    Score = apiStory.Score,
                    CommentCount = apiStory.Descendants
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(m_cacheDuration);
                m_cache.Set(storyCacheKey, story, cacheEntryOptions);

                GetStorySemaphore.Release();
                return story;
            }
            catch (Exception e)
            {
                if(acquired) GetStorySemaphore.Release();
                m_logger.LogError(e, $"Error fetching story: {storyId}");
                return null;
            }
        }
    }
}
