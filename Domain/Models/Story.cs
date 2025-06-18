namespace HackerNews.Core.Models
{
    public class Story
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.MinValue;
        public int Score { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
    }
}
