using HackerNews.Core.Models;
using HackerNews.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController(ILogger<StoryController> logger, IStoryService storyService) : ControllerBase
    {
        private readonly ILogger<StoryController> m_logger = logger;
        private readonly IStoryService m_storyService = storyService;

        [HttpGet("best")]
        public async Task<IEnumerable<Story>> GetAsync([FromQuery] int quantity)
        {
            return await m_storyService.GetBestStoriesAsync(quantity);
        }
    }
}
