using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Trip.Commands;
using Kledex;
using Kledex.Domain;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Action = Duber.Domain.Trip.Commands.Action;
using ViewModel = Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class TripController : Controller
    {
        private readonly IDispatcher _dispatcher;
        private readonly IMapper _mapper;
        private readonly IRepository<Domain.Trip.Model.Trip> _repository;

        public TripController(IDispatcher dispatcher, IMapper mapper, IRepository<Domain.Trip.Model.Trip> repository)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Returns a trip that matches with the specified id.
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns>Returns a trip that matches with the specified id.</returns>
        /// <response code="200">Returns a Trip object that matches with the specified id.</response>
        [HttpGet("{tripId}")]
        [ProducesResponseType(typeof(ViewModel.Trip), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTrip(Guid tripId)
        {
            var trip = await _repository.GetByIdAsync(tripId);
            var tripViewModel = _mapper.Map<ViewModel.Trip>(trip);

            if (tripViewModel == null)
                return NotFound();

            return Ok(tripViewModel);
        }

        /// <summary>
        /// Creates a new trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns the newly created trip identifier.</returns>
        /// <response code="201">Returns the newly created trip identifier.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateTrip([FromBody]ViewModel.CreateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter
            var domainCommand = _mapper.Map<CreateTripCommand>(command);
            domainCommand.AggregateRootId = Guid.NewGuid();
            await _dispatcher.SendAsync(domainCommand);
            return Created(HttpContext.Request.GetUri().AbsoluteUri, domainCommand.AggregateRootId);
        }

        /// <summary>
        /// Accepts the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("accept")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AcceptTrip([FromBody]ViewModel.UpdateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.Accept;

            await _dispatcher.SendAsync(domainCommand);
            return Ok();
        }

        /// <summary>
        /// Starts the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("start")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> StartTrip([FromBody]ViewModel.UpdateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.Start;

            await _dispatcher.SendAsync(domainCommand);
            return Ok();
        }

        /// <summary>
        /// Cancels the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("cancel")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CancelTrip([FromBody]ViewModel.UpdateTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.Cancel;

            await _dispatcher.SendAsync(domainCommand);
            return Ok();
        }

        /// <summary>
        /// Updates the current location for the specified trip.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateCurrentLocation([FromBody]ViewModel.UpdateCurrentLocationTripCommand command)
        {
            // BadRequest and InternalServerError could be throw in HttpGlobalExceptionFilter, and also by ValidatorActionFilter due to the UpdateTripCommandValidator.
            var domainCommand = _mapper.Map<UpdateTripCommand>(command);
            domainCommand.Action = Action.UpdateCurrentLocation;

            await _dispatcher.SendAsync(domainCommand);
            return Ok();
        }
    }
}