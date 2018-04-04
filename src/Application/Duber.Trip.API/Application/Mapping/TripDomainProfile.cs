using AutoMapper;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.Mapping
{
    public class TripDomainProfile : Profile
    {
        public TripDomainProfile()
        {
            CreateMap<Domain.Trip.Model.Trip, ViewModel.Trip>();
        }
    }
}
