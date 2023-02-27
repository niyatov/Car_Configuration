using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Car_Configuration.Entities.Configurations;

public class ModelConfiguration : EntityBaseConfiguration<Model>
{
    public override void Configure(EntityTypeBuilder<Model> builder)
    {
        base.Configure(builder);
        builder.Property(b => b.FolderPath).HasColumnType("varchar(50)").IsRequired();
        builder.HasMany(c=>c.ColorModels)
            .WithOne(e=>e.Model).IsRequired();
        builder.HasMany(c => c.Wheels)
            .WithOne(e => e.Model).IsRequired();
    }
}