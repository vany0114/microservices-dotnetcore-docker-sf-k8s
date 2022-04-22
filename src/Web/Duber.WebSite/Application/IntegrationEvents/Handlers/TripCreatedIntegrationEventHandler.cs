using System;
using System.Threading.Tasks;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Repository;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;
using Duber.WebSite.Models;
using Microsoft.Extensions.Logging;

namespace Duber.WebSite.Application.IntegrationEvents.Handlers
{
    public class TripCreatedIntegrationEventHandler : IIntegrationEventHandler<TripCreatedIntegrationEvent>
    {
        private readonly IReportingRepository _reportingRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TripCreatedIntegrationEventHandler> _logger;

        public TripCreatedIntegrationEventHandler(IReportingRepository reportingRepository, IDriverRepository driverRepository, IUserRepository userRepository, ILogger<TripCreatedIntegrationEventHandler> logger)
        {
            _reportingRepository = reportingRepository ?? throw new ArgumentNullException(nameof(reportingRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger;
        }

        public async Task Handle(TripCreatedIntegrationEvent @event)
        {
            _logger.LogInformation("TripCreatedIntegrationEvent handled");
            var existingTrip = await _reportingRepository.GetTripAsync(@event.TripId);
            if (existingTrip != null) return;

            _logger.LogInformation($"Trip {@event.TripId} has been created.");
            var driver = await _driverRepository.GetDriverAsync(@event.DriverId);
            var user = await _userRepository.GetUserAsync(@event.UserTripId);

            var newTrip = new Trip
            {
                Id = @event.TripId,
                Created = @event.CreationDate,
                PaymentMethod = @event.PaymentMethod.Name,
                Status = "Created",
                Model = @event.VehicleInformation.Model,
                Brand = @event.VehicleInformation.Brand,
                Plate = @event.VehicleInformation.Plate,
                DriverId = @event.DriverId,
                DriverName = driver.Name,
                From = @event.From.Description,
                To = @event.To.Description,
                UserId = @event.UserTripId,
                UserName = user.Name
            };

            try
            {
                await _reportingRepository.AddTripAsync(newTrip);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to create the Trip: {@event.TripId}", ex);
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }
    }
}
