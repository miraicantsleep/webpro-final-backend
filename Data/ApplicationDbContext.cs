using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
using pweb_eas.Models.Entities;

namespace pweb_eas.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public ApplicationDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Configuration.GetConnectionString("pwebEASDb"));
        }
        public DbSet<User> Users { get; set; }
    }
}