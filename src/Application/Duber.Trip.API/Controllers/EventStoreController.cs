using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Duber.Trip.API.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Documents;

namespace Duber.Trip.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class EventStoreController : Controller
    {
        private readonly IEventStoreRepository _repository;

        public EventStoreController(IEventStoreRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Returns all of the Aggregates
        /// </summary>
        /// <returns>Returns all of the Aggregates</returns>
        /// <response code="200">Returns a list of AggregateDocument object.</response>
        [Route("aggregates")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AggregateDocument>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAggreagates()
        {
            var aggregates = await _repository.GetAggregatesAsync();

            if (aggregates == null)
                return NotFound();

            return Ok(aggregates);
        }

        /// <summary>
        /// Returns all events that matches with the specified aggregate id.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns>Returns all events that matches with the specified aggregate id.</returns>
        /// <response code="200">Returns a list of EventDocument object.</response>
        [Route("eventsbyaggregate")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventDocument>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEventsByAggregate(Guid aggregateId)
        {
            var events = await _repository.GetEventsAsync(aggregateId);

            if (events == null)
                return NotFound();

            return Ok(events);
        }
    }
}