using Car_Configuration.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Car_Configuration.Data;

public class AppDbContext : IdentityDbContext<User, UserRole, int>
{
    public DbSet<Model> Models { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<ColorModel> ColorModels { get; set; }
    public DbSet<Wheel> Wheels { get; set; }
    public DbSet<WheelColorModel> WheelColorModels { get; set; }
    public DbSet<UserWheelColor> UserWheelColors { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}