using AutoMapper;
using Duber.Domain.Invoice.Events;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.Mapping
{
    public class InvoiceEventsProfile : Profile
    {
        public InvoiceEventsProfile()
        {
            CreateMap<InvoiceCreatedDomainEvent, InvoiceCreatedIntegrationEvent>()
                .ConstructUsing(x => new InvoiceCreatedIntegrationEvent(
                    x.InvoiceId,
                    x.Fee,
                    x.Total,
                    x.TripId
                ));

            CreateMap<InvoicePaidDomainEvent, InvoicePaidIntegrationEvent>()
                .ConstructUsing(x => new InvoicePaidIntegrationEvent(
                    x.InvoiceId,
                    (PaymentStatus)(int)x.Status,
                    x.CardNumber,
                    x.CardType,
                    x.TripId
                ));
        }
    }
}
