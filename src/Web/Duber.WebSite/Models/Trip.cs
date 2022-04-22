using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Duber.WebSite.Models
{
    public class Trip
    {
        // trip information
        public Guid Id { get; set; }

        public string Status { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? Ended { get; set; }

        public double? Distance { get; set; }

        public TimeSpan? Duration { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string PaymentMethod { get; set; }

        // user information
        public int UserId { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }

        // driver information
        public int DriverId { get; set; }

        [Display(Name = "Driver")]
        public string DriverName { get; set; }

        // vehicle information
        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        // invoice information
        public Guid? InvoiceId { get; set; }

        [Display(Name = "Booking Fee")]
        public decimal? Fee { get; set; }

        [Display(Name = "Trip Fare")]
        public decimal? Fare { get; set; }

        [NotMapped]
        public decimal? Total => Fee + Fare;

        // payment information
        [Display(Name = "Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Credit Card Number")]
        public string CardNumber { get; set; }

        [Display(Name = "Type")]
        public string CardType { get; set; }
    }
}
