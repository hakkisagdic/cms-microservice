using Microsoft.EntityFrameworkCore;
using ContentService.Core.Entities;

namespace ContentService.Infrastructure.Data;

public class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Content> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Body)
                .IsRequired();

            entity.Property(e => e.Summary)
                .HasMaxLength(500);

            entity.Property(e => e.FeaturedImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.MetaTitle)
                .HasMaxLength(200);

            entity.Property(e => e.MetaDescription)
                .HasMaxLength(500);

            entity.Property(e => e.Tags)
                .HasMaxLength(500);

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.AuthorName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Slug)
                .HasMaxLength(250);

            entity.HasIndex(e => e.Slug)
                .IsUnique();

            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => e.PublishedAt);

            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.Property(e => e.Type)
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(e => e.ViewCount)
                .HasDefaultValue(0);

            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0);

            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false);

            entity.Property(e => e.AllowComments)
                .HasDefaultValue(true);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
