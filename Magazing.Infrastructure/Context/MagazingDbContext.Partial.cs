using Magazing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magazing.Infrastructure.Context;

public partial class MagazingDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>().Property(e => e.Version).IsRowVersion();
        modelBuilder.Entity<Product>().Property(e => e.Version).IsRowVersion();
    }
}
