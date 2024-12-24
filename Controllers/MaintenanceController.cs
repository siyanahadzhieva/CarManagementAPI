using CarManagementApi.Data;
using CarManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly CarManagementContext _context;

        public MaintenanceController(CarManagementContext context)
        {
            _context = context;
        }

        // GET: api/maintenance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceRequest>>> GetMaintenanceRequests()
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Car)
                .Include(m => m.Garage)
                .ToListAsync();
        }

        // POST: api/maintenance
        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> CreateMaintenanceRequest(MaintenanceRequest request)
        {
            // Проверка за капацитет на сервиза
            var requestsOnDate = await _context.MaintenanceRequests
                .Where(m => m.GarageId == request.GarageId && m.ScheduledDate == request.ScheduledDate)
                .CountAsync();

            var garage = await _context.Garages.FindAsync(request.GarageId);

            if (garage == null || requestsOnDate >= garage.Capacity)
            {
                return BadRequest("No available capacity for the selected garage and date.");
            }

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMaintenanceRequests), new { id = request.Id }, request);
        }
    }
}
