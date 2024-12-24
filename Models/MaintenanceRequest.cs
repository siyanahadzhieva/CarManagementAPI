namespace CarManagementApi.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; }
        public string ServiceType { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

}
