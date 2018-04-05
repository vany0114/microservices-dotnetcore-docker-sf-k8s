namespace Duber.Trip.API.Application.Model
{
    public class CreateTripCommand
    {
        public int UserId { get; set; }

        public int DriverId { get; set; }

        public Location From { get; set; }

        public Location To { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }
}
