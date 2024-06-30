using Microsoft.EntityFrameworkCore;

namespace HashDog.Models
{
    public class Database : DbContext
    {
        public DbSet<OutpostEntry> Outposts { get; set; }
        public DbSet<FileEntry> Files { get; set; }
        public DbSet<ArchiveEntry> Archives { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure SQLite database
            optionsBuilder.UseSqlite(@"Data Source=C:\Users\Zyd\testing\HashDog.db");
    
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<FileEntry>()
                .HasKey(f => f.Id);
            modelBuilder.Entity<OutpostEntry>()
                .HasKey(o => o.Id);
            modelBuilder.Entity<ArchiveEntry>()
                .HasKey(a => a.Id); 


            // Configure relationships
            modelBuilder.Entity<FileEntry>()
                .HasOne(f => f.OutpostEntry)
                .WithMany(o => o.Files)
                .HasForeignKey(f => f.OutpostEntryId);

            modelBuilder.Entity<ArchiveEntry>()
                .HasOne(a => a.FileEntry)
                .WithMany()
                .HasForeignKey(a => a.FileEntryId);

            modelBuilder.Entity<ArchiveEntry>()
                .HasOne(a => a.OutpostEntry)
                .WithMany()
                .HasForeignKey(a => a.OutpostEntryId);
        }


    }
}
