using Microsoft.EntityFrameworkCore;
using TinyApiUrl.Entity;

namespace TinyApiUrl.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Url> Urls { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
