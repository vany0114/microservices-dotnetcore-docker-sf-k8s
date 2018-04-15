using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Duber.WebSite.Models
{
    public class TripRequestModel : IValidatableObject
    {
        [Required]
        public string User { get; set; }

        [Required]
        public string Driver { get; set; }

        public List<SelectListItem> Users { get; set; }

        public List<SelectListItem> Drivers { get; set; }

        public List<SelectListItem> Origins { get; set; }

        public List<SelectListItem> Destinations { get; set; }

        public List<LocationModel> Places { get; set; }

        public List<LocationModel> Directions { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From == To)
            {
                yield return new ValidationResult("The origin can't be the same to destination", new[] { "From" });
            }
        }
    }

    public class LocationModel
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Description { get; set; }
    }
}
