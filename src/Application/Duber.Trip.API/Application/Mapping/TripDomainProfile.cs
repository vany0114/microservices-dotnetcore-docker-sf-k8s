using AutoMapper;
using Duber.Domain.SharedKernel.Model;
using Duber.Domain.Trip.Model;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripDomainProfile : Profile
    {
        public TripDomainProfile()
        {
            CreateMap<PaymentMethod, ViewModel.PaymentMethod>();
            CreateMap<Location, ViewModel.Location>();
            CreateMap<TripStatus, ViewModel.TripStatus>();
            CreateMap<VehicleInformation, ViewModel.VehicleInformation>();
            CreateMap<Rating, ViewModel.Rating>();
            CreateMap<Domain.Trip.Model.Trip, ViewModel.Trip>();
        }
    }
}
