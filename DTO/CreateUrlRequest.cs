namespace TinyApiUrl.DTO
{
    public class CreateUrlRequest
    {
        public string OriginalUrl { get; set; }
        public bool IsPrivate { get; set; }
    }
}
