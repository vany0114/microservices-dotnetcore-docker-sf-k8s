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
        private PaymentInfo _paymentInfo;

        public decimal Fee => _fee;

        public PaymentMethod PaymentMethod => _paymentMethod;

        public decimal Total => _total;

        public Guid InvoiceId => _invoiceId;

        public TripInformation TripInformation => _tripInformation;

        public DateTime Created => _created;

        public PaymentInfo PaymentInfo => _paymentInfo;

        // to Dapper mapping.
        protected Invoice(Guid invoiceId, decimal fee, decimal total, int paymentMethodId, Guid tripId, double distance,
            TimeSpan duration, DateTime created, int tripStatusId, int status, string cardNumber, string cardType, int userId)
        {
            _invoiceId = invoiceId;
            _created = created;
            _tripInformation = new TripInformation(tripId, duration, distance, tripStatusId);
            _fee = fee;
            _paymentMethod = PaymentMethod.From(paymentMethodId);
            _total = total;

            if (userId != default(int))
                _paymentInfo = new PaymentInfo(userId, (PaymentStatus)status, cardNumber, cardType);
        }

        public Invoice(int paymentMethodId, Guid tripId, TimeSpan duration, double distance, int tripStatusId)
        {
            if (paymentMethodId == default(int)) throw new InvoiceDomainArgumentNullException(nameof(paymentMethodId));

            _invoiceId = Guid.NewGuid();
            _created = DateTime.UtcNow;
            _paymentMethod = PaymentMethod.From(paymentMethodId);
            _tripInformation = new TripInformation(tripId, duration, distance, tripStatusId);
            GetFee();
            GetTotal();

            AddDomainEvent(new InvoiceCreatedDomainEvent(_invoiceId, _fee, _total, Equals(_paymentMethod, PaymentMethod.CreditCard), _tripInformation.Id));
        }

        public void ProcessPayment(PaymentInfo paymentInfo)
        {
            if (!Equals(_paymentMethod, PaymentMethod.CreditCard))
                throw new InvoiceDomainInvalidOperationException("Invalid payment method to process.");

            if (_total == 0)
                throw new InvoiceDomainInvalidOperationException("This invoice doesn't have any charges.");

            _paymentInfo = paymentInfo;
            AddDomainEvent(new InvoicePaidDomainEvent(_invoiceId, _paymentInfo.Status, _paymentInfo.CardNumber, _paymentInfo.CardType, _tripInformation.Id));
        }

        private void GetFee()
        {
            // let's say there is this bussines rule to get the fee.
            if (Equals(_tripInformation.Status, TripStatus.Cancelled))
            {
                _fee = 4;
            }
            else if (_tripInformation.DistanceToKilometers() < 5)
            {
                _fee = 3;
            }
            else if (_tripInformation.DurationToMinutes() < 15)
            {
                _fee = 2;
            }
        }

        private void GetTotal()
        {
            // let's say there is formula to get the total.
            // a strategy pattern could be a good call to calculate de total based on the trip status.
            if (Equals(_tripInformation.Status, TripStatus.Cancelled))
            {
                // if the user cancels the trip after 5 minutes, it charges a value proportional to the minutes.
                if (_tripInformation.DurationToMinutes() > 5)
                {
                    _total = _fee + (decimal)_tripInformation.DurationToMinutes();
                }
                else if (_tripInformation.DurationToMinutes() > 2 && _tripInformation.DurationToMinutes() <= 5)
                {
                    // if the user cancels the trip between the 2nd and 5th minute, it charges a fixed value.
                    _fee = 0;
                    _total = 2;
                }
                else
                {
                    // if the user cancels the trip before 2 minutes it doesn't charge anything
                    _fee = 0;
                    _total = 0;
                }
            }
            else
            {
                _total = (decimal)(_tripInformation.DistanceToKilometers() * _tripInformation.DurationToMinutes() + (double)_fee);

                if (_total <= 0)
                    throw new InvoiceDomainInvalidOperationException("There was an error calculating the invoice total");
            }
        }
    }
}
