using MemDrawer.Domain;
using Microsoft.EntityFrameworkCore;

namespace MemDrawer.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Image> Images => Set<Image>();
}