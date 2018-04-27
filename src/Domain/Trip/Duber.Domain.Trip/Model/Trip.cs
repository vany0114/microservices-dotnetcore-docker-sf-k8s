using System;
using Duber.Domain.SharedKernel.Model;
using Duber.Domain.Trip.Events;
using Duber.Domain.Trip.Exceptions;
using GeoCoordinatePortable;
using Weapsy.Cqrs.Domain;
using Action = Duber.Domain.Trip.Events.Action;
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Trip.Model
{
    public class Trip : AggregateRoot
    {
        private int _userId;
        private int _driverId;
        private Location _from;
        private Location _to;
        private Location _currentLocation;
        private DateTime _create;
        private DateTime? _start;
        private DateTime? _end;
        private TripStatus _status;
        private VehicleInformation _vehicleInformation;
        private Rating _rating;
        private PaymentMethod _paymentMethod;

        public int UserId => _userId;

        public int DriverId => _driverId;

        public Location From => _from;

        public Location To => _to;

        public Location CurrentLocation => _currentLocation;

        public DateTime? Created => _create;

        public DateTime? Started => _start;

        public DateTime? End => _end;

        public TripStatus Status => _status;

        public VehicleInformation VehicleInformation => _vehicleInformation;

        public Rating Rating => _rating;

        public PaymentMethod PaymentMethod => _paymentMethod;

        public TimeSpan? Duration => GetDuration();

        public double Distance => GetDistance();

        // public empty constructor is required for Weapsy.CQRS
        public Trip()
        {
        }

        public Trip(Guid id, int userId, int driverId, Location from, Location to, PaymentMethod paymentMethod, string plate, string brand, string model) : base(id)
        {
            if (userId <= 0) throw new TripDomainArgumentNullException(nameof(userId));
            if (driverId <= 0) throw new TripDomainArgumentNullException(nameof(driverId));
            if (string.IsNullOrWhiteSpace(plate)) throw new TripDomainArgumentNullException(nameof(plate));
            if (string.IsNullOrWhiteSpace(brand)) throw new TripDomainArgumentNullException(nameof(brand));
            if (string.IsNullOrWhiteSpace(model)) throw new TripDomainArgumentNullException(nameof(model));
            if (from == null) throw new TripDomainArgumentNullException(nameof(from));
            if (to == null) throw new TripDomainArgumentNullException(nameof(to));

            if (Equals(from, to)) throw new TripDomainInvalidOperationException("Destination and origin can't be the same.");

            _paymentMethod = paymentMethod ?? throw new TripDomainArgumentNullException(nameof(paymentMethod));
            _create = DateTime.UtcNow;
            _status = TripStatus.Created;
            _userId = userId;
            _driverId = driverId;
            _from = from;
            _to = to;
            _vehicleInformation = new VehicleInformation(plate, brand, model);

            AddEvent(new TripCreatedDomainEvent
            {
                AggregateRootId = Id,
                VehicleInformation = _vehicleInformation,
                UserTripId = _userId,
                DriverId = _driverId,
                From = _from,
                To = _to,
                PaymentMethod = _paymentMethod,
                TimeStamp = _create,
                Status = _status
            });
        }

        public void Accept()
        {
            if (!Equals(_status, TripStatus.Created))
                throw new TripDomainInvalidOperationException($"Invalid trip status to accept the trip. Current status: {_status.Name}");

            _status = TripStatus.Accepted;
            AddEvent(new TripUpdatedDomainEvent
            {
                AggregateRootId = Id,
                Action = Action.Accepted,
                Status = _status
            });
        }

        public void Start()
        {
            if (!Equals(_status, TripStatus.Accepted))
                throw new TripDomainInvalidOperationException($"Before to start the trip, it should be accepted. Current status: {_status.Name}");

            _start = DateTime.UtcNow;

            // we're assuming that the driver already picked up the user.
            _status = TripStatus.InCourse;

            AddEvent(new TripUpdatedDomainEvent
            {
                AggregateRootId = Id,
                Action = Action.Started,
                Status = _status,
                Started = _start
            });
        }

        public void FinishEarlier()
        {
            if (!Equals(_status, TripStatus.InCourse))
                throw new TripDomainInvalidOperationException($"Invalid trip status to finish the trip. Current status: {_status.Name}");

            _end = DateTime.UtcNow;
            _status = TripStatus.Finished;
            _to = _currentLocation;

            AddEvent(new TripUpdatedDomainEvent
            {
                AggregateRootId = Id,
                Action = Action.FinishedEarlier,
                Status = _status,
                Started = _start,
                Ended = _end,
                Duration = GetDuration(),
                Distance = GetDistance(),
                PaymentMethod = _paymentMethod,
                UserTripId = _userId
            });
        }

        public void Cancel()
        {
            if (!Equals(_status, TripStatus.Created) || !Equals(_status, TripStatus.Accepted))
                throw new TripDomainInvalidOperationException($"Invalid trip status to cancel the trip. Current status: {_status.Name}");

            _end = DateTime.UtcNow;
            _status = TripStatus.Cancelled;

            //let's say there is a business rule that says when cancelling the rating is 2 for both user an driver.
            _rating = new Rating(2, 2);

            AddEvent(new TripUpdatedDomainEvent
            {
                AggregateRootId = Id,
                Action = Action.Cancelled,
                Status = _status,
                Started = _start,
                Ended = _end,
                PaymentMethod = _paymentMethod,
                Duration = GetDuration(),
                UserTripId = _userId
            });
        }

        public void SetCurrentLocation(Location currentLocation)
        {
            if (!Equals(_status, TripStatus.InCourse))
                throw new TripDomainInvalidOperationException($"Invalid trip status to set the current location. Current status: {_status.Name}");

            _currentLocation = currentLocation ?? throw new TripDomainArgumentNullException(nameof(currentLocation));

            // TODO: handle a tolerance range to determine if current location is the destination
            if (Equals(currentLocation, _to))
            {
                _end = DateTime.UtcNow;
                _status = TripStatus.Finished;
            }

            AddEvent(new TripUpdatedDomainEvent
            {
                AggregateRootId = Id,
                Action = Action.UpdatedCurrentLocation,
                Status = _status,
                Started = _start,
                Ended = _end,
                CurrentLocation = currentLocation,
                Duration = GetDuration(),
                Distance = GetDistance(),
                PaymentMethod = _paymentMethod,
                UserTripId = _userId
            });
        }

        private TimeSpan? GetDuration()
        {
            TimeSpan? duration = null;
            if (_start != null)
            {
                duration = _end?.Subtract(_start.Value);
            }

            return duration;
        }

        private double GetDistance()
        {
            if (_from == null || _to == null)
                return 0;

            var from = new GeoCoordinate(_from.Latitude, _from.Longitude);
            var to = new GeoCoordinate(_to.Latitude, _to.Longitude);
            return from.GetDistanceTo(to);
        }

        // Applies events after load an object from event store. (kinda memento pattern)
        private void Apply(TripCreatedDomainEvent @event)
        {
            Id = @event.AggregateRootId;
            _status = @event.Status;
            _create = @event.TimeStamp;
            _driverId = @event.DriverId;
            _from = @event.From;
            _to = @event.To;
            _userId = @event.UserTripId;
            _vehicleInformation = @event.VehicleInformation;
            _paymentMethod = @event.PaymentMethod;
        }

        private void Apply(TripUpdatedDomainEvent @event)
        {
            _start = @event.Started;
            _end = @event.Ended;
            _status = @event.Status;
            _currentLocation = @event.CurrentLocation;
        }
    }
}
