using AutoMapper;
using Duber.Domain.Trip.Commands;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripCommandsProfile : Profile
    {
        public TripCommandsProfile()
        {
            // we're working with viewmodels in order to don't expose our domain objects, we don't want to expose things like: AggregateRootId, UserId, Source, etc.
            CreateMap<ViewModel.CreateTripCommand, CreateTripCommand>()
                .ForMember(dest => dest.UserTripId, opts => opts.MapFrom(src => src.UserId));

            CreateMap<ViewModel.UpdateTripCommand, UpdateTripCommand>()
                .ForMember(dest => dest.AggregateRootId, opts => opts.MapFrom(src => src.Id));

            CreateMap<ViewModel.UpdateCurrentLocationTripCommand, UpdateTripCommand>()
                .ForMember(dest => dest.AggregateRootId, opts => opts.MapFrom(src => src.Id));
        }
    }
}
