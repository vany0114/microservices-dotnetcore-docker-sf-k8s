using System;
using Duber.Domain.Invoice.Events;
using Duber.Domain.Invoice.Exceptions;
using Duber.Domain.Invoice.Extensions;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.DDD;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Duber.Domain.Invoice.Model
{
    public class Invoice : Entity, IAggregateRoot
    {
        private Guid _invoiceId;
        private decimal _fee = 1;
        private decimal _total;
        private TripInformation _tripInformation;
        private PaymentMethod _paymentMethod;
        private DateTime _created;

        public decimal Fee => _fee;

        public PaymentMethod PaymentMethod => _paymentMethod;

        public decimal Total => _total;

        public Guid InvoiceId => _invoiceId;

        public TripInformation Information => _tripInformation;

        public DateTime Created => _created;

        public Invoice(PaymentMethod paymentMethod, TimeSpan duration, double distance)
        {
            if (duration == default(TimeSpan)) throw new InvoiceDomainArgumentNullException(nameof(duration));
            if (distance == default(double)) throw new InvoiceDomainArgumentNullException(nameof(distance));

            _invoiceId = Guid.NewGuid();
            _created = DateTime.UtcNow;
            _paymentMethod = paymentMethod ?? throw new InvoiceDomainArgumentNullException(nameof(paymentMethod));
            _tripInformation = new TripInformation(duration, distance);
            GetFee();
            GetTotal();

            AddDomainEvent(new InvoiceCreatedDomainEvent(_invoiceId, _fee, _total));
        }

        private void GetFee()
        {
            // let's say there is this bussines rule to get the fee.
            if (Information.DistanceToKilometers() < 5)
            {
                _fee = 2;
            }
            else if (Information.DurationToMinutes() < 15)
            {
                _fee = 3;
            }
        }

        private void GetTotal()
        {
            // let's say there is formula to get the total.
            _total = (decimal)(Information.DistanceToKilometers() * Information.DurationToMinutes() * (double)_fee);

            if (_total <= 0)
                throw new InvoiceDomainInvalidOperationException("There was an error calculating the invoice total");
        }
    }
}
