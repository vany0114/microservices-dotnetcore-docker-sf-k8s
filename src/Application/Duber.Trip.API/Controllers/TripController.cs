using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Trip.Commands;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Weapsy.Cqrs;
using Action = Duber.Domain.Trip.Commands.Action;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class TripController : Controller
    {
        private readonly IDispatcher _dispatcher;
        private readonly IMapper _mapper;
        private readonly Guid _fakeUser = Guid.NewGuid();
        private const string Source = "Duber.Trip.Api";

        public TripController(IDispatcher dispatcher, IMapper mapper)
        {
            _dispatcher = dispatcher;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns the newly created trip identifier.</returns>
        /// <response code="201">Returns the newly created trip identifier.</response>
        [Route("create")]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateTrip([FromBody]ViewModel.CreateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter
            var tripId = Guid.NewGuid();
            var domainCommand = _mapper.Map<CreateTripCommand>(command);
            domainCommand.AggregateRootId = tripId;
            domainCommand.Source = Source;
            domainCommand.UserId = _fakeUser;

            await _dispatcher.SendAndPublishAsync<CreateTripCommand, Domain.Trip.Model.Trip>(domainCommand);
            return Created(HttpContext.Request.GetUri().AbsoluteUri, tripId);
        }

        /// <summary>
        /// Accepts the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Route("accept")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AcceptTrip([FromBody]ViewModel.UpdateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.Accept;
            domainCommand.Source = Source;
            domainCommand.UserId = _fakeUser;

            await _dispatcher.SendAndPublishAsync<UpdateTripCommand, Domain.Trip.Model.Trip>(domainCommand);
            return Ok();
        }

        /// <summary>
        /// Starts the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Route("start")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartTrip([FromBody]ViewModel.UpdateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.Start;
            domainCommand.Source = Source;
            domainCommand.UserId = _fakeUser;

            await _dispatcher.SendAndPublishAsync<UpdateTripCommand, Domain.Trip.Model.Trip>(domainCommand);
            return Ok();
        }

        /// <summary>
        /// Updates the current location for the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Route("updatecurrentlocation")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCurrentLocation([FromBody]ViewModel.UpdateCurrentLocationTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.UpdateCurrentLocation;
            domainCommand.Source = Source;
            domainCommand.UserId = _fakeUser;

            await _dispatcher.SendAndPublishAsync<UpdateTripCommand, Domain.Trip.Model.Trip>(domainCommand);
            return Ok();
        }
    }
}