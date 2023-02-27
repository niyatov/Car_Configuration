using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Car_Configuration.Entities.Configurations;

public class ColorModelConfiguration : IEntityTypeConfiguration<ColorModel>
{
    public virtual void Configure(EntityTypeBuilder<ColorModel> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnType("integer")
            .ValueGeneratedOnAdd();
        builder.HasOne(c => c.Color)
           .WithMany(e => e.ColorModels).IsRequired();
        builder.HasMany(c => c.WheelColors)
           .WithOne(e => e.ColorModel).IsRequired();
    }
}