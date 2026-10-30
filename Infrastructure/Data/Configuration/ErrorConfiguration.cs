using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class ErrorConfiguration : IEntityTypeConfiguration<Error>
{
    public void Configure(EntityTypeBuilder<Error> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ErrorMessage)
            .IsRequired();

        builder.Property(e => e.OcurredAtUtc)
            .IsRequired();
    }
}
