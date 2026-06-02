using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mappings;

public class SaleMapping : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.SaleNumber).HasColumnName("sale_number").HasMaxLength(50).IsRequired();
        builder.Property(s => s.SaleDate).HasColumnName("sale_date").IsRequired()
            .HasConversion(                                                                         
                        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),                          
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));       
        builder.Property(s => s.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(s => s.CustomerName).HasColumnName("customer_name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(s => s.BranchName).HasColumnName("branch_name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.IsCancelled).HasColumnName("is_cancelled").IsRequired();

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Ignore(s => s.TotalAmount);
        //builder.Ignore(s => s.DomainEvents);
    }
}