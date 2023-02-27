using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Car_Configuration.Entities.Configurations;

public class WheelColorModelConfiguration : IEntityTypeConfiguration<WheelColorModel>
{
    public virtual void Configure(EntityTypeBuilder<WheelColorModel> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnType("integer")
            .ValueGeneratedOnAdd();
        builder.Property(b => b.ColorWheelPath).HasColumnType("varchar(100)").IsRequired();
        builder.HasOne(c => c.Wheel)
           .WithMany(e => e.WheelColors).IsRequired();
    }
}