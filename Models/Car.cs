namespace CarManagementApi.Models
{
    public class Car
    {
        public int Id { get; private set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public ICollection<Garage> Garages { get; set; } = new List<Garage>();
    }

}
