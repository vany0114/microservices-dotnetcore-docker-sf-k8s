using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Duber.Domain.Driver.Model;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Model;
using Duber.Domain.User.Repository;
using Duber.Infrastructure.Http;
using Duber.Infrastructure.Resilience.Http;
using Duber.WebSite.Extensions;
using Duber.WebSite.Infrastructure.Repository;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
// ReSharper disable ForCanBeConvertedToForeach

namespace Duber.WebSite.Controllers
{
    public class TripController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;
        private readonly ResilientHttpClient _httpClient;
        private readonly IDriverRepository _driverRepository;
        private readonly IReportingRepository _reportingRepository;
        private readonly IOptions<TripApiSettings> _tripApiSettings;
        private readonly Dictionary<SelectListItem, LocationModel> _originsAndDestinations;

        public TripController(IUserRepository userRepository,
            IDriverRepository driverRepository,
            IMemoryCache cache,
            ResilientHttpClient httpClient,
            IOptions<TripApiSettings> tripApiSettings,
            IReportingRepository reportingRepository)
        {
            _userRepository = userRepository;
            _driverRepository = driverRepository;
            _cache = cache;
            _httpClient = httpClient;
            _tripApiSettings = tripApiSettings;
            _reportingRepository = reportingRepository;

            _originsAndDestinations = new Dictionary<SelectListItem, LocationModel>
            {
                {
                    new SelectListItem { Text = "Poblado's Park" },
                    new LocationModel { Latitude = 6.210292869847029, Longitude = -75.57115852832794, Description = "Poblado's Park" }
                },
                {
                    new SelectListItem { Text = "Lleras Park" },
                    new LocationModel { Latitude = 6.2087793817882515, Longitude = -75.56776275426228, Description = "Lleras Park" }
                },
                {
                    new SelectListItem { Text = "Sabaneta Park" },
                    new LocationModel { Latitude = 6.151584634798451, Longitude = -75.61546325683594, Description = "Sabaneta Park" }
                },
                {
                    new SelectListItem { Text = "The executive bar" },
                    new LocationModel { Latitude = 6.252063572976704, Longitude = -75.56599313040351, Description = "The executive bar" }
                },
            };
        }

        public async Task<IActionResult> Index()
        {
            var drivers = await GetDrivers();
            var users = await GetUsers();

            var model = new TripRequestModel
            {
                Drivers = drivers.ToSelectList(),
                Users = users.ToSelectList(),
                User = users.FirstOrDefault()?.Id.ToString(),
                Driver = drivers.FirstOrDefault()?.Id.ToString(),
                Origins = _originsAndDestinations.Keys.Select(x => x).ToList(),
                Destinations = _originsAndDestinations.Keys.Select(x => x).ToList(),
                From = _originsAndDestinations.Keys.Select(x => x).ToList().FirstOrDefault()?.Text,
                To = _originsAndDestinations.Keys.Select(x => x).ToList().LastOrDefault()?.Text,
                Places = _originsAndDestinations.Values.Select(x => x).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SimulateTrip(TripRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.AllErrors());
            
            var tripID = await CreateTrip(model);
            await AcceptOrStartTrip(_tripApiSettings.Value.AcceptUrl, tripID, model.ConnectionId);
            await AcceptOrStartTrip(_tripApiSettings.Value.StartUrl, tripID, model.ConnectionId);

            for (var index = 0; index < model.Directions.Count; index += 5)
            {
                var direction = model.Directions[index];
                if (index + 5 >= model.Directions.Count)
                    direction = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.To);

                await UpdateTripLocation(tripID, direction, model.ConnectionId);
            }

            return Ok();
        }

        public async Task<IActionResult> TripsByUser()
        {
            var users = await GetUsers();
            var usersModel = users.Select(x => new UserModel
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                NumberPhone = x.NumberPhone
            });

            return View(usersModel.ToList());
        }

        public async Task<IActionResult> TripsByUserId(int id)
        {
            try
            {
                var trips = await _reportingRepository.GetTripsByUserAsync(id);
                return View("UserTrips", trips.ToList());
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }

        public async Task<IActionResult> TripById(Guid id)
        {
            try
            {
                var trip = await _reportingRepository.GetTripAsync(id);
                return View("TripDetails", trip);
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }

        public async Task<IActionResult> TripsByDriver()
        {
            var drivers = await GetDrivers();
            var driversModel = drivers.Select(x => new DriverModel()
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                NumberPhone = x.PhoneNumber
            });

            return View(driversModel.ToList());
        }

        public async Task<IActionResult> TripsByDriverId(int id)
        {
            try
            {
                var trips = await _reportingRepository.GetTripsByDriverAsync(id);
                return View("DriverTrips", trips.ToList());
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }

        public async Task<IActionResult> Test()
        {
            var model = new TripRequestModel
            {
                Driver = (await GetDrivers()).FirstOrDefault()?.Id.ToString(),
                User = (await GetUsers()).FirstOrDefault()?.Id.ToString(),
                From = "Poblado's Park",
                To = "Sabaneta Park"
            };

            await CreateTrip(model);
            return Ok();
        }

        private async Task<IList<Driver>> GetDrivers()
        {
            var drivers = await GetDataFromCache("drivers", () => _driverRepository.GetDriversAsync());
            return drivers;
        }

        private async Task<IList<User>> GetUsers()
        {
            var users = await GetDataFromCache("users", () => _userRepository.GetUsersAsync());
            return users;
        }

        private async Task<T> GetDataFromCache<T>(string cacheKey, Func<Task<T>> action)
        {
            if (!_cache.TryGetValue(cacheKey, out T result))
            {
                result = await action();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, result, cacheEntryOptions);
            }

            return result;
        }

        private async Task<Guid> CreateTrip(TripRequestModel model)
        {
            var drivers = await GetDrivers();
            var users = await GetUsers();
            var driverInfo = drivers.SingleOrDefault(x => x.Id == int.Parse(model.Driver));
            var userInfo = users.SingleOrDefault(x => x.Id == int.Parse(model.User));

            var uri = new Uri(new Uri(_tripApiSettings.Value.BaseUrl), _tripApiSettings.Value.CreateUrl);
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new JsonContent(new
                {
                    userId = int.Parse(model.User),
                    driverId = int.Parse(model.Driver),
                    from = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.From),
                    to = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.To),
                    plate = driverInfo?.CurrentVehicle.Plate,
                    brand = driverInfo?.CurrentVehicle.Brand,
                    model = driverInfo?.CurrentVehicle.Model,
                    paymentMethod = userInfo?.PaymentMethod,
                    connectionId = model.ConnectionId
                })
            };

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Guid>(await response.Content.ReadAsStringAsync());
        }

        private async Task AcceptOrStartTrip(string action, Guid tripId, string connectionId)
        {
            var uri = new Uri(new Uri(_tripApiSettings.Value.BaseUrl), action);
            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new JsonContent(new { id = tripId.ToString(), connectionId })
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private async Task UpdateTripLocation(Guid tripId, LocationModel location, string connectionId)
        {
            var uri = new Uri(new Uri(_tripApiSettings.Value.BaseUrl), _tripApiSettings.Value.UpdateCurrentLocationUrl);
            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new JsonContent(new
                {
                    id = tripId,
                    currentLocation = new { latitude = location.Latitude, longitude = location.Longitude, connectionId }
                })
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}