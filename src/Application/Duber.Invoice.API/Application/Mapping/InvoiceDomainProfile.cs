using AutoMapper;
using Duber.Domain.Invoice.Model;
using Duber.Domain.SharedKernel.Model;
using ViewModel = Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.Mapping
{
    public class InvoiceDomainProfile : Profile
    {
        public InvoiceDomainProfile()
        {
            CreateMap<Domain.Invoice.Model.Invoice, ViewModel.Invoice>();
            CreateMap<PaymentMethod, ViewModel.PaymentMethod>();
            CreateMap<TripStatus, ViewModel.TripStatus>();
            CreateMap<TripInformation, ViewModel.TripInformation>();
            CreateMap<PaymentInfo, ViewModel.PaymentInfo>();
        }
    }
}
