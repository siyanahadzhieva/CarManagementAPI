using CarManagementApi.Data;
using CarManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarManagementApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly CarManagementContext _context;

        public CarsController(CarManagementContext context)
        {
            _context = context;
        }

        // GET: api/cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            return await _context.Cars.Include(c => c.Garages).ToListAsync();
        }

        // GET: api/cars/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await _context.Cars.Include(c => c.Garages).FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            return car;
        }

        // POST: api/cars
        [HttpPost]
        public async Task<ActionResult<Car>> CreateCar(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCar), new { id = car.Id }, car);
        }

        // PUT: api/cars/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, Car car)
        {
            if (id != car.Id)
            {
                return BadRequest();
            }

            _context.Entry(car).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(id))
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

        // DELETE: api/cars/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Car>>> FilterCars([FromQuery] string brand, [FromQuery] int? garageId, [FromQuery] int? startYear, [FromQuery] int? endYear)
        {
            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(c => c.Make.ToLower() == brand.ToLower());
            }

            if (startYear.HasValue && endYear.HasValue)
            {
                query = query.Where(c => c.Year >= startYear && c.Year <= endYear);
            }

            var cars = await query.ToListAsync();

            if (!cars.Any())
            {
                return NotFound("No cars found for the specified filters.");
            }

            return Ok(cars);
        }

        [HttpPost("{carId}/addGarage/{garageId}")]
        public async Task<IActionResult> AddGarageToCar(int carId, int garageId)
        {
            var car = await _context.Cars.Include(c => c.Garages).FirstOrDefaultAsync(c => c.Id == carId);
            var garage = await _context.Garages.FindAsync(garageId);

            if (car == null || garage == null)
            {
                return NotFound("Car or Garage not found.");
            }

            if (car.Garages.Contains(garage))
            {
                return BadRequest("Garage is already associated with this car.");
            }

            car.Garages.Add(garage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
