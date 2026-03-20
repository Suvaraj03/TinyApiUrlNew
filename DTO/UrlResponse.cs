namespace TinyApiUrl.DTO
{
    public class UrlResponse
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortUrl { get; set; }
        public int ClickCount { get; set; }
    }
}
