using CarManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManagementApi.Data
{
    public class CarManagementContext : DbContext
    {
        public CarManagementContext(DbContextOptions<CarManagementContext> options)
            : base(options) { }

        public DbSet<Garage> Garages { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
    }

}
