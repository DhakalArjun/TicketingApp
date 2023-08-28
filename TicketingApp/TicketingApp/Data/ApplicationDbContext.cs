using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketingApp.Models;

namespace TicketingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketLocation> TicketLocations { get; set; }
        public DbSet<Comment> Comment { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.UseLazyLoadingProxies();
            builder.Entity<TicketPriority>().HasData(
                new TicketPriority {Id=1, TktPriority = "Critical" },
                 new TicketPriority {Id=2, TktPriority = "High" },
                 new TicketPriority {Id=3, TktPriority = "Medium" },
                 new TicketPriority {Id=4, TktPriority = "Low" }
                );
            
            builder.Entity<TicketStatus>().HasData(
                new TicketStatus { StatusId = 1, Status = "Not assigned" },
                new TicketStatus { StatusId = 2, Status = "Assigned" },
                new TicketStatus { StatusId = 3, Status = "Resolved" },
                new TicketStatus { StatusId = 4, Status = "Not Resolvable" },
                new TicketStatus { StatusId = 5, Status = "Resolved & Closed" },
                new TicketStatus { StatusId = 6, Status = "Not Resolvable & Closed" }
                );
            builder.Entity<TicketCategory>().HasData(
                new TicketCategory { CategoryId = 1, Category = "IT Service", SubCategory = "Desktop Support - OS service pack upgrade" },
                new TicketCategory { CategoryId = 2, Category = "IT Service", SubCategory = "Desktop Support - PC problem" },
                new TicketCategory { CategoryId = 3, Category = "IT Service", SubCategory = "Desktop Support - Software issue" },
                new TicketCategory { CategoryId = 4, Category = "IT Service", SubCategory = "Laptop Support - Laptop battery/charger issue" },
                new TicketCategory { CategoryId = 5, Category = "IT Service", SubCategory = "Laptop Support - Laptop hardware service" },
                new TicketCategory { CategoryId = 6, Category = "IT Service", SubCategory = "Network Support - Connectivity issue" },
                new TicketCategory { CategoryId = 7, Category = "IT Service", SubCategory = "Network Support - Internet speed issue" },
                new TicketCategory { CategoryId = 8, Category = "IT Service", SubCategory = "Printer Support - Paper issue" },
                new TicketCategory { CategoryId = 9, Category = "IT Service", SubCategory = "Printer Support - Printer cartridge issue" },
                new TicketCategory { CategoryId = 10, Category = "IT Service", SubCategory = "Printer Support - Printing problem" },
                new TicketCategory { CategoryId = 11, Category = "IT Service", SubCategory = "Security Support - Anti virus issue" },
                new TicketCategory { CategoryId = 12, Category = "IT Service", SubCategory = "Security Support - Brower issue" },
                new TicketCategory { CategoryId = 13, Category = "IT Service", SubCategory = "Security Support - System clean" },
                new TicketCategory { CategoryId = 14, Category = "IT Service", SubCategory = "Server Support - Application performance" },
                new TicketCategory { CategoryId = 15, Category = "IT Service", SubCategory = "Server Support - Server Performance" }
                );
            builder.Entity<TicketLocation>().HasData(
                new TicketLocation { LocId = 1, Location = "Department A" },
                new TicketLocation { LocId = 2, Location = "Department B" },
                new TicketLocation { LocId = 3, Location = "Department C" },
                new TicketLocation { LocId = 4, Location = "Department D" },
                new TicketLocation { LocId = 5, Location = "Department E" },
                new TicketLocation { LocId = 6, Location = "Department F" },
                new TicketLocation { LocId = 7, Location = "Department G" },
                new TicketLocation { LocId = 8, Location = "Department H" },
                new TicketLocation { LocId = 9, Location = "Department I" },
                new TicketLocation { LocId = 10, Location = "Department J" }
                );
            base.OnModelCreating( builder );            
        
        }        
    }
}