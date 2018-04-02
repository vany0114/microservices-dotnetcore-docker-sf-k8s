using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Trip.Commands;
using Duber.Domain.Trip.Events;
using Duber.Domain.Trip.Model;
using Microsoft.AspNetCore.Mvc;
using Weapsy.Cqrs;
using Weapsy.Cqrs.Domain;
using Action = Duber.Domain.Trip.Commands.Action;

namespace Duber.Trip.API.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IDispatcher _dispatcher;
        private readonly IRepository<Domain.Trip.Model.Trip> _repository;

        public ValuesController(IDispatcher dispatcher, IRepository<Domain.Trip.Model.Trip> repository)
        {
            _dispatcher = dispatcher;
            _repository = repository;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            // test code:
            var tripId = Guid.NewGuid();

            // create trip
            await _dispatcher.SendAndPublishAsync<CreateTrip, Domain.Trip.Model.Trip>(new CreateTrip
            {
                AggregateRootId = tripId,
                DriverId = 1,
                UserTripId = 2,
                Plate = "HAN248",
                Brand = "Chevrolet",
                Model = "2015",
                From = new Location(6.279292, -75.579843),
                To = new Location(6.271095497377606, -75.57648539543152)
            });

            // accept trip
            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.Accept
            });

            // start trip
            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.Start
            });

            // updated current location
            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.UpdateCurrentLocation,
                CurrentLocation = new Location(6.278, -75.577)
            });

            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.UpdateCurrentLocation,
                CurrentLocation = new Location(6.275, -75.577)
            });

            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.UpdateCurrentLocation,
                CurrentLocation = new Location(6.272, -75.577)
            });

            await _dispatcher.SendAndPublishAsync<UpdatedTrip, Domain.Trip.Model.Trip>(new UpdatedTrip
            {
                AggregateRootId = tripId,
                Action = Action.UpdateCurrentLocation,
                CurrentLocation = new Location(6.271095497377606, -75.57648539543152)
            });

            var trip = await _repository.GetByIdAsync(tripId);

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
