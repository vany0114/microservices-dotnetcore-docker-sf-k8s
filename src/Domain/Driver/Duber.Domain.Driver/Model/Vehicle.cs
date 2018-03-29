using Duber.Domain.Driver.Exceptions;
using Duber.Infrastructure.DDD;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable NotAccessedField.Local

namespace Duber.Domain.Driver.Model
{
    public class Vehicle : Entity
    {
        private string _plate;
        private string _brand;
        private string _model;
        private bool _active;
        private int _typeId;

        public string Plate => _plate;

        public string Brand => _brand;

        public string Model => _model;

        public bool Active => _active;

        // EF navigation property
        public VehicleType Type { get; private set; }

        protected Vehicle()
        {
        }

        // contructor is internal due to doesn't make sense create a vehicle itself without a driver (in the Duber business context)
        // so the only way to create vehicles is throught the Driver aggregate root.
        internal Vehicle(string plate, string brand, string model, VehicleType type)
        {
            _plate = !string.IsNullOrWhiteSpace(plate) ? plate : throw new DriverDomainException(nameof(plate));
            _brand = !string.IsNullOrWhiteSpace(brand) ? brand : throw new DriverDomainException(nameof(brand));
            _model = !string.IsNullOrWhiteSpace(model) ? model : throw new DriverDomainException(nameof(model));
            _active = true;
            _typeId = type.Id;
        }

        public void Inactivate()
        {
            if (!_active)
                throw new DriverDomainException($"The vehicule {_plate} is already inactive");

            _active = false;
        }

        public void Activate()
        {
            if (!_active)
                throw new DriverDomainException($"The vehicule {_plate} is already active");

            _active = true;
        }
    }
}
