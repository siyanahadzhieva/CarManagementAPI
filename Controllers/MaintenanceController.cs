using CarManagementApi.Data;
using CarManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarManagementApi.Controllers
{
    [Route("[controller]")]
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

        // GET: api/maintenance/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Car)
                .Include(m => m.Garage)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
            {
                return NotFound("Maintenance request not found.");
            }

            return request;
        }

        // POST: api/maintenance
        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> CreateMaintenanceRequest(MaintenanceRequest request)
        {
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

            return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = request.Id }, request);
        }

        // PUT: api/maintenance/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaintenanceRequest(int id, MaintenanceRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("Request ID mismatch.");
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceRequestExists(id))
                {
                    return NotFound("Maintenance request not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/maintenance/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceRequest(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);

            if (request == null)
            {
                return NotFound("Maintenance request not found.");
            }

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.Id == id);
        }

        [HttpGet("monthly-statistics")]
        public async Task<ActionResult<IEnumerable<object>>> GetMonthlyStatistics([FromQuery] int garageId, [FromQuery] DateTime startMonth, [FromQuery] DateTime endMonth)
        {
            var garage = await _context.Garages.FindAsync(garageId);
            if (garage == null)
            {
                return NotFound("Garage not found.");
            }

            var statistics = await _context.MaintenanceRequests
                .Where(m => m.GarageId == garageId && m.ScheduledDate >= startMonth && m.ScheduledDate <= endMonth)
                .GroupBy(m => new { m.ScheduledDate.Year, m.ScheduledDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRequests = g.Count()
                })
                .ToListAsync();

            var allMonths = Enumerable.Range(0, ((endMonth.Year - startMonth.Year) * 12) + endMonth.Month - startMonth.Month + 1)
                .Select(offset => new DateTime(startMonth.Year, startMonth.Month, 1).AddMonths(offset))
                .Select(date => new
                {
                    Year = date.Year,
                    Month = date.Month,
                    TotalRequests = 0
                })
                .ToList();

            foreach (var stat in statistics)
            {
                var match = allMonths.FirstOrDefault(m => m.Year == stat.Year && m.Month == stat.Month);
                if (match != null)
                {
                    allMonths.Remove(match);
                    allMonths.Add(stat);
                }
            }

            return Ok(allMonths.OrderBy(m => m.Year).ThenBy(m => m.Month));
        }

    }
}
