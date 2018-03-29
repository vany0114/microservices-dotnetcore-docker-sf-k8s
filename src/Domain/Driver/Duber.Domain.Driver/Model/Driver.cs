using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Domain.Driver.Exceptions;
using Duber.Infrastructure.DDD;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable NotAccessedField.Local

namespace Duber.Domain.Driver.Model
{
    public class Driver : Entity, IAggregateRoot
    {
        private string _name;
        private string _email;
        private string _phoneNumber;
        private int _rating;
        private Vehicle _currentVehicle;
        private int _statusId;

        // Using a private collection field, better for DDD Aggregate's encapsulation
        // so _vehicles cannot be added from "outside the AggregateRoot" directly to the collection,
        // but only through the method AddVehicle() which includes behaviour.
        private readonly List<Vehicle> _vehicles;

        public string Name => _name;

        public string Email => _email;

        public string PhoneNumber => _phoneNumber;

        public int Rating => _rating;

        public Vehicle CurrentVehicle => GetCurrentVehicle();

        // EF navigation property
        public DriverStatus Status { get; private set; }

        // Using List<>.AsReadOnly() 
        // This will create a read only wrapper around the private list so is protected against "external updates".
        // It's much cheaper than .ToList() because it will not have to copy all items in a new collection. (Just one heap alloc for the wrapper instance)
        //https://msdn.microsoft.com/en-us/library/e78dcd75(v=vs.110).aspx 
        public IReadOnlyCollection<Vehicle> Vehicles => _vehicles;

        protected Driver()
        {
            _vehicles = new List<Vehicle>();
        }

        public Driver(
            string name,
            string email,
            int rating,
            string vehiclePlate,
            string vehicleBrand,
            string vehicleModel,
            VehicleType vehicleType,
            string phoneNumber = null) : this()
        {
            if (rating < 0 || rating > 5) throw new DriverDomainException("Driver rating should be between 1 and 5");

            _name = !string.IsNullOrWhiteSpace(name) ? name : throw new DriverDomainException(nameof(name));
            _email = !string.IsNullOrWhiteSpace(email) ? email : throw new DriverDomainException(nameof(email));
            _phoneNumber = phoneNumber;
            _rating = rating;
            _statusId = DriverStatus.Active.Id;

            _currentVehicle = new Vehicle(vehiclePlate, vehicleBrand, vehicleModel, vehicleType);
            _vehicles.Add(_currentVehicle);
        }

        public void AddVehicle(string vehiclePlate, string vehicleBrand, string vehicleModel, VehicleType vehicleType)
        {
            if (CurrentVehicle == null)
                throw new DriverDomainException($"Driver {_name} doesn't have an active vehicle.");

            _currentVehicle.Inactivate();
            var newVehicle = new Vehicle(vehiclePlate, vehicleBrand, vehicleModel, vehicleType);
            _vehicles.Add(newVehicle);
            _currentVehicle = newVehicle;
        }

        public void Inactivate()
        {
            // business rules here
            _statusId = DriverStatus.Inactive.Id;
        }

        private Vehicle GetCurrentVehicle()
        {
            try
            {
                _currentVehicle = _currentVehicle ?? _vehicles.SingleOrDefault(x => x.Active);
                return _currentVehicle;
            }
            catch (InvalidOperationException)
            {
                throw new DriverDomainException("There are more than one active vehicles.");
            }
        }
    }
}
