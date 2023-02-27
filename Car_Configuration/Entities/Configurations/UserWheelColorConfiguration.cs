using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Car_Configuration.Entities.Configurations;

public class UserWheelColorConfiguration : IEntityTypeConfiguration<UserWheelColor>
{
    public virtual void Configure(EntityTypeBuilder<UserWheelColor> builder)
    {
        builder.HasKey(k => new { k.UserId, k.WheelColorModelId });
        builder.HasOne(c => c.User)
           .WithMany(e => e.UserWheelColors).IsRequired();
        builder.HasOne(s => s.WheelColorModel)
            .WithMany(e => e.UserWheelColors).IsRequired();
    }
}