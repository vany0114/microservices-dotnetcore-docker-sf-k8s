using System;
using System.Collections.Generic;
using Duber.Domain.Invoice.Exceptions;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.DDD;
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Invoice.Model
{
    public class TripInformation : ValueObject
    {
        public TimeSpan Duration { get; }

        public double Distance { get; }

        public Guid Id { get; }

        public TripStatus Status { get; }

        internal TripInformation(Guid id, TimeSpan duration, double distance, int statusId)
        {
            if (statusId == default(int)) throw new InvoiceDomainArgumentNullException(nameof(statusId));
            if (duration == default(TimeSpan)) throw new InvoiceDomainArgumentNullException(nameof(duration));

            Id = id;
            Duration = duration;
            Status = TripStatus.From(statusId);

            if (!Equals(Status, TripStatus.Finished) && !Equals(Status, TripStatus.Cancelled))
                throw new InvoiceDomainInvalidOperationException("Invalid trip status to create an invoice");

            if (distance <= 0 && !Equals(Status, TripStatus.Cancelled))
                throw new InvoiceDomainArgumentNullException(nameof(distance));

            Distance = Equals(Status, TripStatus.Cancelled) ? 0 : distance;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Duration;
            yield return Distance;
            yield return Id;
            yield return Status;
        }
    }
}
