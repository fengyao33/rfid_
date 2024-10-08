using Microsoft.EntityFrameworkCore;

namespace rfidServer_C_.AppDbContext;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<tblRFIDHistory> TblRFIDHistory { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tblRFIDHistory>(entity =>
        {
            entity.HasKey(e => e.Rid);  
            entity.Property(e => e.Rid)
                  .HasDefaultValueSql("NEWID()");  
        });
    }

    public class tblRFIDHistory
    {
        public Guid Rid { get; set; } 
        public string? TagID { get; set; } 
        public string? Location { get; set; } 
        public string? Action { get; set; } 
        public DateTime? BDate { get; set; } 
        public string? BUser { get; set; } 
        public string? Success { get; set; } 
        public string? Reason { get; set; }
    }

}
