using AutoMapper;
using Duber.Domain.Trip.Events;
using Duber.Trip.API.Application.IntegrationEvents;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripEventsProfile : Profile
    {
        public TripEventsProfile()
        {
            CreateMap<TripUpdatedDomainEvent, TripUpdatedIntegrationEvent>();
            CreateMap<TripCreatedDomainEvent, TripCreatedIntegrationEvent>();
        }
    }
}
