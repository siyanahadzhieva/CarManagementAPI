using CarManagementApi.Data;
using CarManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GaragesController : ControllerBase
    {
        private readonly CarManagementContext _context;

        public GaragesController(CarManagementContext context)
        {
            _context = context;
        }

        // GET: api/garages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Garage>>> GetGarages()
        {
            return await _context.Garages.ToListAsync();
        }

        // GET: api/garages/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Garage>> GetGarage(int id)
        {
            var garage = await _context.Garages.FindAsync(id);

            if (garage == null)
            {
                return NotFound();
            }

            return garage;
        }

        // POST: api/garages
        [HttpPost]
        public async Task<ActionResult<Garage>> CreateGarage(Garage garage)
        {
            _context.Garages.Add(garage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGarage), new { id = garage.Id }, garage);
        }

        // PUT: api/garages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGarage(int id, Garage garage)
        {
            if (id != garage.Id)
            {
                return BadRequest();
            }

            _context.Entry(garage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GarageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/garages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGarage(int id)
        {
            var garage = await _context.Garages.FindAsync(id);
            if (garage == null)
            {
                return NotFound();
            }

            _context.Garages.Remove(garage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GarageExists(int id)
        {
            return _context.Garages.Any(e => e.Id == id);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Garage>>> GetGaragesByCity([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }

            var garages = await _context.Garages
                .Where(g => g.City.ToLower() == city.ToLower())
                .ToListAsync();

            if (!garages.Any())
            {
                return NotFound("No garages found for the specified city.");
            }

            return Ok(garages);
        }

        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<IEnumerable<object>>> GetGarageStatistics(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var garage = await _context.Garages.FindAsync(id);

            if (garage == null)
            {
                return NotFound($"Garage with ID {id} not found.");
            }

            var statistics = await _context.MaintenanceRequests
                .Where(m => m.GarageId == id && m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
                .GroupBy(m => m.ScheduledDate)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRequests = g.Count(),
                    AvailableCapacity = garage.Capacity - g.Count()
                })
                .ToListAsync();

            return Ok(statistics);
        }

    }
}
