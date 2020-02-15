using System;
using AutoMapper;
using Duber.Domain.SharedKernel.Model;
using Duber.Domain.Trip.Commands;
using Duber.Domain.Trip.Model;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripCommandsProfile : Profile
    {
        public TripCommandsProfile()
        {
            CreateMap<ViewModel.PaymentMethod, PaymentMethod>();
            CreateMap<ViewModel.Location, Location>();

            // we're working with viewmodels in order to don't expose our domain objects, we don't want to expose things like: AggregateRootId, UserId, Source, etc.
            CreateMap<ViewModel.CreateTripCommand, CreateTripCommand>()
                .ForMember(dest => dest.UserTripId, opts => opts.MapFrom(src => src.UserId));

            CreateMap<ViewModel.UpdateTripCommand, UpdateTripCommand>()
                .ForMember(dest => dest.AggregateRootId, opts => opts.MapFrom(src => src.Id))
                .AfterMap((src, dest) => dest.Id = Guid.NewGuid()); // command id must be unique, the id which comes in src.Id is the aggregate root (trip id)


            CreateMap<ViewModel.UpdateCurrentLocationTripCommand, UpdateTripCommand>()
                .ForMember(dest => dest.AggregateRootId, opts => opts.MapFrom(src => src.Id))
                .AfterMap((src, dest) => dest.Id = Guid.NewGuid()); // command id must be unique, the id which comes in src.Id is the aggregate root (trip id
        }
    }
}
