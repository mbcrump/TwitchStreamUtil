using Microsoft.EntityFrameworkCore;

namespace TwitchStreamUtil.EntityFramework
{
    /// <summary>
    /// Context for entity framework
    /// </summary>
    public class TitleContext : DbContext
    {
        public TitleContext(DbContextOptions<TitleContext> options)
            : base(options)
        { }

        public DbSet<TitleEf> Titles { get; set; }
    }
}