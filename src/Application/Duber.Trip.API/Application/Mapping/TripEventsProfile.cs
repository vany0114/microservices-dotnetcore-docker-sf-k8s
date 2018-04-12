using AutoMapper;
using Duber.Domain.Trip.Events;
using Duber.Trip.API.Application.IntegrationEvents;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripEventsProfile : Profile
    {
        public TripEventsProfile()
        {
            CreateMap<TripUpdatedDomainEvent, TripUpdatedIntegrationEvent>()
                .ConstructUsing(x => new TripUpdatedIntegrationEvent(
                    x.AggregateRootId,
                    (IntegrationEvents.Action)(int)x.Action,
                    new TripStatus { Id = x.Status.Id, Name = x.Status.Name },
                    x.Started,
                    x.Ended,
                    x.CurrentLocation == null ? null : new Location { Latitude = x.CurrentLocation.Latitude, Longitude = x.CurrentLocation.Longitude },
                    x.Distance,
                    x.Duration
                ));

            CreateMap<TripCreatedDomainEvent, TripCreatedIntegrationEvent>()
                .ConstructUsing(x => new TripCreatedIntegrationEvent(
                    x.AggregateRootId,
                    x.UserTripId,
                    x.DriverId,
                    new Location { Latitude = x.From.Latitude, Longitude = x.From.Longitude, Description = x.From.Description},
                    new Location { Latitude = x.To.Latitude, Longitude = x.To.Longitude, Description = x.To.Description},
                    new VehicleInformation
                    {
                        Brand = x.VehicleInformation.Brand,
                        Model = x.VehicleInformation.Model,
                        Plate = x.VehicleInformation.Plate
                    },
                    new PaymentMethod { Id = x.PaymentMethod.Id, Name = x.PaymentMethod.Name }
                ));
        }
    }
}
