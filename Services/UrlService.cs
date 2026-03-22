using Microsoft.EntityFrameworkCore;
using TinyApiUrl.Data;
using TinyApiUrl.DTO;
using TinyApiUrl.Entity;
using TinyApiUrl.Helper;

namespace TinyApiUrl.Services
{
    public class UrlService : IUrlService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContext;

        public UrlService(AppDbContext db, IHttpContextAccessor httpContext)
        {
            _db = db;
            _httpContext = httpContext;
        }

        //public async Task<UrlResponse> CreateShortUrlAsync(CreateUrlRequest request)
        //{
        //    if (!Uri.IsWellFormedUriString(request.OriginalUrl, UriKind.Absolute))
        //        throw new Exception("Invalid URL");

        //    string shortCode;

        //    do
        //    {
        //        shortCode = ShortCodeGenerator.Generate();
        //    }
        //    while (await _db.Urls.AnyAsync(x => x.ShortCode == shortCode));

        //    var url = new Url
        //    {
        //        OriginalUrl = request.OriginalUrl,
        //        ShortCode = shortCode,
        //        IsPrivate = request.IsPrivate
        //    };

        //    _db.Urls.Add(url);
        //    await _db.SaveChangesAsync();

        //    //var baseUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}";
        //    var baseUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}";
        //    return new UrlResponse
        //    {
        //        Id = url.Id,
        //        OriginalUrl = url.OriginalUrl,
        //        ShortUrl = $"{baseUrl}/{url.ShortCode}",
        //        ClickCount = url.ClickCount
        //    };
        //}

        public async Task<UrlResponse> CreateShortUrlAsync(CreateUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OriginalUrl))
                throw new ArgumentException("URL cannot be empty");

            //  Add scheme if missing
            if (!request.OriginalUrl.StartsWith("http://") &&
                !request.OriginalUrl.StartsWith("https://"))
            {
                request.OriginalUrl = "https://" + request.OriginalUrl;
            }

            // Validate properly
            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Invalid URL");
            }

            string shortCode;

            do
            {
                shortCode = ShortCodeGenerator.Generate();
            }
            while (await _db.Urls.AnyAsync(x => x.ShortCode == shortCode));

            var url = new Url
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                IsPrivate = request.IsPrivate
            };

            _db.Urls.Add(url);
            await _db.SaveChangesAsync();

            var baseUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}";

            return new UrlResponse
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = $"{baseUrl}/{url.ShortCode}",
                ClickCount = url.ClickCount
            };
        }



        public async Task<List<UrlResponse>> GetPublicUrlsAsync()
        {
            var urls = await _db.Urls
                .Where(x => !x.IsPrivate)
                .ToListAsync();
            var baseUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}";

            return urls.Select(u => new UrlResponse
            {
                Id = u.Id,
                OriginalUrl = u.OriginalUrl,
                ShortUrl = $"{baseUrl}/{u.ShortCode}",
                ClickCount = u.ClickCount
            }).ToList();
        }

        public async Task<Url> GetByShortCodeAsync(string code)
        {
            var url = await _db.Urls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (url == null) return null;
            //if (url != null)
            //{
            //    url.ClickCount++;
            //    await _db.SaveChangesAsync();
            //}

            //return url;


            // IMPORTANT: increment click count here
            url.ClickCount++;
            await _db.SaveChangesAsync();

            return url;




        }

        public async Task<bool> DeleteAsync(int id)
        {
            var url = await _db.Urls.FindAsync(id);
            if (url == null) return false;

            _db.Urls.Remove(url);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<UrlResponse>> SearchAsync(string query)
        {
            var urls = await _db.Urls
                .Where(x => x.OriginalUrl.Contains(query) || x.ShortCode.Contains(query))
                .ToListAsync();

            return urls.Select(u => new UrlResponse
            {
                Id = u.Id,
                OriginalUrl = u.OriginalUrl,
                ShortUrl = u.ShortCode,
                ClickCount = u.ClickCount
            }).ToList();
        }

        /*  public async Task<List<UrlResponse>> SearchAsync(string query)
          {
              var urlsQuery = _db.Urls
                  .Where(x => !x.IsPrivate); // don't expose private

              if (!string.IsNullOrWhiteSpace(query))
              {
                  urlsQuery = urlsQuery
                      .Where(x => x.OriginalUrl.Contains(query));
              }

              var urls = await urlsQuery.ToListAsync();

              return urls.Select(MapToResponse).ToList();
          }*/

        private UrlResponse MapToResponse(Url u)
        {
            var baseUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}";

            return new UrlResponse
            {
                Id = u.Id,
                OriginalUrl = u.OriginalUrl,
                ShortUrl = $"{baseUrl}/{u.ShortCode}",
                ClickCount = u.ClickCount
            };
        }
    }
}
