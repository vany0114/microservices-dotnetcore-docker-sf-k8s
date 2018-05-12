using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Duber.Domain.Driver.Model;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Model;
using Duber.Domain.User.Repository;
using Duber.Infrastructure.Resilience.Http;
using Duber.WebSite.Extensions;
using Duber.WebSite.Hubs;
using Duber.WebSite.Infrastructure.Repository;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
// ReSharper disable ForCanBeConvertedToForeach

namespace Duber.WebSite.Controllers
{
    public class TripController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;
        private readonly ResilientHttpInvoker _httpInvoker;
        private readonly IHubContext<TripHub> _hubContext;
        private readonly IDriverRepository _driverRepository;
        private readonly IReportingRepository _reportingRepository;
        private readonly IOptions<TripApiSettings> _tripApiSettings;
        private readonly Dictionary<SelectListItem, LocationModel> _originsAndDestinations;

        public TripController(IUserRepository userRepository,
            IDriverRepository driverRepository, 
            IMemoryCache cache,
            ResilientHttpInvoker httpInvoker,
            IOptions<TripApiSettings> tripApiSettings,
            IHubContext<TripHub> hubContext,
            IReportingRepository reportingRepository)
        {
            _userRepository = userRepository;
            _driverRepository = driverRepository;
            _cache = cache;
            _httpInvoker = httpInvoker;
            _tripApiSettings = tripApiSettings;
            _hubContext = hubContext;
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
            await _hubContext.Clients.All.SendAsync("NotifyTrip", "Created");

            await AcceptOrStartTrip(_tripApiSettings.Value.AcceptUrl, tripID);
            await _hubContext.Clients.All.SendAsync("NotifyTrip", "Accepted");

            await AcceptOrStartTrip(_tripApiSettings.Value.StartUrl, tripID);
            await _hubContext.Clients.All.SendAsync("NotifyTrip", "Started");

            for (var index = 0; index < model.Directions.Count; index = index + 5)
            {
                var direction = model.Directions[index];
                if (index + 5 > model.Directions.Count)
                    direction = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.To);

                await UpdateTripLocation(tripID, direction);
                await _hubContext.Clients.All.SendAsync("UpdateCurrentPosition", direction);
            }

            await _hubContext.Clients.All.SendAsync("NotifyTrip", "Finished");
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
            var trips = await _reportingRepository.GetTripsByUserAsync(id);
            return View("UserTrips", trips.ToList());
        }

        public async Task<IActionResult> TripById(Guid id)
        {
            var trip = await _reportingRepository.GetTripAsync(id);
            return View("TripDetails", trip);
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
            var trips = await _reportingRepository.GetTripsByDriverAsync(id);
            return View("DriverTrips", trips.ToList());
        }

        public async Task<IActionResult> Test()
        {
            var model = new TripRequestModel
            {
                Driver = (await GetDrivers()).FirstOrDefault()?.Id.ToString(),
                User = (await GetUsers()).FirstOrDefault()?.Id.ToString(),
                From = "Poblado's Park",
                To = "Sabaneta Park",
                Directions = JsonConvert.DeserializeObject<List<LocationModel>>(GetDirectionsFromText())
            };
            
            var tripID = await CreateTrip(model);
            await AcceptOrStartTrip(_tripApiSettings.Value.AcceptUrl, tripID);
            await AcceptOrStartTrip(_tripApiSettings.Value.StartUrl, tripID);

            for (var index = 0; index < model.Directions.Count; index = index + 15)
            {
                var direction = model.Directions[index];
                if (index + 15 > model.Directions.Count)
                    direction = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.To);

                await UpdateTripLocation(tripID, direction);
            }

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

            var createRequestRresponse = await _httpInvoker.InvokeAsync(async () =>
            {
                var client = new RestClient(_tripApiSettings.Value.BaseUrl);
                var request = new RestRequest(_tripApiSettings.Value.CreateUrl, Method.POST);
                request.AddJsonBody(new
                {
                    userId = int.Parse(model.User),
                    driverId = int.Parse(model.Driver),
                    from = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.From),
                    to = _originsAndDestinations.Values.SingleOrDefault(x => x.Description == model.To),
                    plate = driverInfo?.CurrentVehicle.Plate,
                    brand = driverInfo?.CurrentVehicle.Brand,
                    model = driverInfo?.CurrentVehicle.Model,
                    paymentMethod = userInfo?.PaymentMethod
                });

                return await client.ExecuteTaskAsync(request);
            });

            if (createRequestRresponse.StatusCode != HttpStatusCode.Created)
                throw new InvalidOperationException("There was an error with Trip service", createRequestRresponse.ErrorException);

            return JsonConvert.DeserializeObject<Guid>(createRequestRresponse.Content);
        }

        private async Task AcceptOrStartTrip(string action, Guid tripId)
        {
            var createRequestRresponse = await _httpInvoker.InvokeAsync(async () =>
            {
                var client = new RestClient(_tripApiSettings.Value.BaseUrl);
                var request = new RestRequest(action, Method.PUT);
                request.AddJsonBody(new { id = tripId.ToString() });

                return await client.ExecuteTaskAsync(request);
            });

            if (createRequestRresponse.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("There was an error with Trip service", createRequestRresponse.ErrorException);
        }

        private async Task UpdateTripLocation(Guid tripId, LocationModel location)
        {
            var createRequestRresponse = await _httpInvoker.InvokeAsync(async () =>
            {
                var client = new RestClient(_tripApiSettings.Value.BaseUrl);
                var request = new RestRequest(_tripApiSettings.Value.UpdateCurrentLocationUrl, Method.PUT);
                request.AddJsonBody(new
                {
                    id = tripId,
                    currentLocation = new { latitude = location.Latitude, longitude = location.Longitude }
                });

                return await client.ExecuteTaskAsync(request);
            });

            if (createRequestRresponse.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("There was an error with Trip service", createRequestRresponse.ErrorException);
        }

        private string GetDirectionsFromText() => "[{\"Latitude\":6.2103800000000007,\"Longitude\":-75.57134,\"Description\":null},{\"Latitude\":6.21048,\"Longitude\":-75.57129,\"Description\":null},{\"Latitude\":6.2106900000000005,\"Longitude\":-75.5712,\"Description\":null},{\"Latitude\":6.2108500000000006,\"Longitude\":-75.571120000000008,\"Description\":null},{\"Latitude\":6.2108500000000006,\"Longitude\":-75.571120000000008,\"Description\":null},{\"Latitude\":6.2107300000000008,\"Longitude\":-75.570810000000009,\"Description\":null},{\"Latitude\":6.2106100000000009,\"Longitude\":-75.570510000000013,\"Description\":null},{\"Latitude\":6.2105900000000007,\"Longitude\":-75.570450000000008,\"Description\":null},{\"Latitude\":6.2105700000000006,\"Longitude\":-75.5704,\"Description\":null},{\"Latitude\":6.2105000000000006,\"Longitude\":-75.5702,\"Description\":null},{\"Latitude\":6.2104300000000006,\"Longitude\":-75.570010000000011,\"Description\":null},{\"Latitude\":6.2103500000000009,\"Longitude\":-75.5698,\"Description\":null},{\"Latitude\":6.2102800000000009,\"Longitude\":-75.56959,\"Description\":null},{\"Latitude\":6.2102,\"Longitude\":-75.569390000000013,\"Description\":null},{\"Latitude\":6.2101200000000008,\"Longitude\":-75.56918,\"Description\":null},{\"Latitude\":6.2101200000000008,\"Longitude\":-75.56918,\"Description\":null},{\"Latitude\":6.21049,\"Longitude\":-75.56904,\"Description\":null},{\"Latitude\":6.21086,\"Longitude\":-75.56889000000001,\"Description\":null},{\"Latitude\":6.21086,\"Longitude\":-75.56889000000001,\"Description\":null},{\"Latitude\":6.2108900000000009,\"Longitude\":-75.56899,\"Description\":null},{\"Latitude\":6.21093,\"Longitude\":-75.56908,\"Description\":null},{\"Latitude\":6.21093,\"Longitude\":-75.56908,\"Description\":null},{\"Latitude\":6.21095,\"Longitude\":-75.569160000000011,\"Description\":null},{\"Latitude\":6.2109700000000005,\"Longitude\":-75.569250000000011,\"Description\":null},{\"Latitude\":6.21098,\"Longitude\":-75.56928,\"Description\":null},{\"Latitude\":6.2109900000000007,\"Longitude\":-75.56931,\"Description\":null},{\"Latitude\":6.2110600000000007,\"Longitude\":-75.56953,\"Description\":null},{\"Latitude\":6.2111300000000007,\"Longitude\":-75.56974000000001,\"Description\":null},{\"Latitude\":6.2111500000000008,\"Longitude\":-75.56981,\"Description\":null},{\"Latitude\":6.2111800000000006,\"Longitude\":-75.57002,\"Description\":null},{\"Latitude\":6.2112200000000009,\"Longitude\":-75.57016,\"Description\":null},{\"Latitude\":6.2112500000000006,\"Longitude\":-75.57025,\"Description\":null},{\"Latitude\":6.21131,\"Longitude\":-75.57038,\"Description\":null},{\"Latitude\":6.2114100000000008,\"Longitude\":-75.57053,\"Description\":null},{\"Latitude\":6.2114300000000009,\"Longitude\":-75.57057,\"Description\":null},{\"Latitude\":6.2114600000000006,\"Longitude\":-75.570600000000013,\"Description\":null},{\"Latitude\":6.21152,\"Longitude\":-75.57065,\"Description\":null},{\"Latitude\":6.21166,\"Longitude\":-75.570730000000012,\"Description\":null},{\"Latitude\":6.2117,\"Longitude\":-75.57075,\"Description\":null},{\"Latitude\":6.2117600000000008,\"Longitude\":-75.5708,\"Description\":null},{\"Latitude\":6.2117900000000006,\"Longitude\":-75.57084,\"Description\":null},{\"Latitude\":6.2117900000000006,\"Longitude\":-75.57084,\"Description\":null},{\"Latitude\":6.2118400000000005,\"Longitude\":-75.57097,\"Description\":null},{\"Latitude\":6.2119500000000007,\"Longitude\":-75.57123,\"Description\":null},{\"Latitude\":6.2121200000000005,\"Longitude\":-75.57161,\"Description\":null},{\"Latitude\":6.2122100000000007,\"Longitude\":-75.57189000000001,\"Description\":null},{\"Latitude\":6.21231,\"Longitude\":-75.572160000000011,\"Description\":null},{\"Latitude\":6.2124700000000006,\"Longitude\":-75.57254,\"Description\":null},{\"Latitude\":6.21257,\"Longitude\":-75.57272,\"Description\":null},{\"Latitude\":6.2127000000000008,\"Longitude\":-75.572970000000012,\"Description\":null},{\"Latitude\":6.21278,\"Longitude\":-75.57313,\"Description\":null},{\"Latitude\":6.2128200000000007,\"Longitude\":-75.57325,\"Description\":null},{\"Latitude\":6.21283,\"Longitude\":-75.57332000000001,\"Description\":null},{\"Latitude\":6.2128400000000008,\"Longitude\":-75.57356,\"Description\":null},{\"Latitude\":6.2128400000000008,\"Longitude\":-75.57375,\"Description\":null},{\"Latitude\":6.21285,\"Longitude\":-75.573910000000012,\"Description\":null},{\"Latitude\":6.2129900000000005,\"Longitude\":-75.574400000000011,\"Description\":null},{\"Latitude\":6.2129900000000005,\"Longitude\":-75.574400000000011,\"Description\":null},{\"Latitude\":6.2129800000000008,\"Longitude\":-75.57447,\"Description\":null},{\"Latitude\":6.2129900000000005,\"Longitude\":-75.57453000000001,\"Description\":null},{\"Latitude\":6.213,\"Longitude\":-75.57459,\"Description\":null},{\"Latitude\":6.21302,\"Longitude\":-75.57468,\"Description\":null},{\"Latitude\":6.21304,\"Longitude\":-75.574790000000007,\"Description\":null},{\"Latitude\":6.21304,\"Longitude\":-75.5749,\"Description\":null},{\"Latitude\":6.2130500000000008,\"Longitude\":-75.575030000000012,\"Description\":null},{\"Latitude\":6.2130500000000008,\"Longitude\":-75.575200000000009,\"Description\":null},{\"Latitude\":6.2130800000000006,\"Longitude\":-75.57541,\"Description\":null},{\"Latitude\":6.21311,\"Longitude\":-75.57558,\"Description\":null},{\"Latitude\":6.2131500000000006,\"Longitude\":-75.57571,\"Description\":null},{\"Latitude\":6.21318,\"Longitude\":-75.57584,\"Description\":null},{\"Latitude\":6.2132200000000006,\"Longitude\":-75.576000000000008,\"Description\":null},{\"Latitude\":6.21325,\"Longitude\":-75.57616,\"Description\":null},{\"Latitude\":6.2132700000000005,\"Longitude\":-75.57621,\"Description\":null},{\"Latitude\":6.2132900000000006,\"Longitude\":-75.576270000000008,\"Description\":null},{\"Latitude\":6.2133,\"Longitude\":-75.57634,\"Description\":null},{\"Latitude\":6.21332,\"Longitude\":-75.57641000000001,\"Description\":null},{\"Latitude\":6.21335,\"Longitude\":-75.576540000000008,\"Description\":null},{\"Latitude\":6.2133800000000008,\"Longitude\":-75.57667,\"Description\":null},{\"Latitude\":6.21349,\"Longitude\":-75.577090000000013,\"Description\":null},{\"Latitude\":6.2135900000000008,\"Longitude\":-75.57752,\"Description\":null},{\"Latitude\":6.2136700000000005,\"Longitude\":-75.57804,\"Description\":null},{\"Latitude\":6.2137100000000007,\"Longitude\":-75.578250000000011,\"Description\":null},{\"Latitude\":6.2137300000000009,\"Longitude\":-75.57837,\"Description\":null},{\"Latitude\":6.21375,\"Longitude\":-75.578480000000013,\"Description\":null},{\"Latitude\":6.21377,\"Longitude\":-75.57858,\"Description\":null},{\"Latitude\":6.21377,\"Longitude\":-75.57858,\"Description\":null},{\"Latitude\":6.2138500000000008,\"Longitude\":-75.57875,\"Description\":null},{\"Latitude\":6.2139000000000006,\"Longitude\":-75.578870000000009,\"Description\":null},{\"Latitude\":6.21398,\"Longitude\":-75.578970000000012,\"Description\":null},{\"Latitude\":6.21403,\"Longitude\":-75.579000000000008,\"Description\":null},{\"Latitude\":6.2140600000000008,\"Longitude\":-75.57902,\"Description\":null},{\"Latitude\":6.2141100000000007,\"Longitude\":-75.57903,\"Description\":null},{\"Latitude\":6.214150000000001,\"Longitude\":-75.57902,\"Description\":null},{\"Latitude\":6.2141800000000007,\"Longitude\":-75.57902,\"Description\":null},{\"Latitude\":6.2142100000000005,\"Longitude\":-75.579000000000008,\"Description\":null},{\"Latitude\":6.21426,\"Longitude\":-75.57898,\"Description\":null},{\"Latitude\":6.2143200000000007,\"Longitude\":-75.578920000000011,\"Description\":null},{\"Latitude\":6.2143400000000009,\"Longitude\":-75.57889,\"Description\":null},{\"Latitude\":6.2143700000000006,\"Longitude\":-75.57884,\"Description\":null},{\"Latitude\":6.2143900000000007,\"Longitude\":-75.578790000000012,\"Description\":null},{\"Latitude\":6.2144,\"Longitude\":-75.57875,\"Description\":null},{\"Latitude\":6.2144,\"Longitude\":-75.57868,\"Description\":null},{\"Latitude\":6.2143900000000007,\"Longitude\":-75.57863,\"Description\":null},{\"Latitude\":6.21438,\"Longitude\":-75.57858,\"Description\":null},{\"Latitude\":6.2143500000000005,\"Longitude\":-75.57855,\"Description\":null},{\"Latitude\":6.2142800000000005,\"Longitude\":-75.57847000000001,\"Description\":null},{\"Latitude\":6.21424,\"Longitude\":-75.578420000000008,\"Description\":null},{\"Latitude\":6.2142000000000008,\"Longitude\":-75.57838000000001,\"Description\":null},{\"Latitude\":6.2140200000000005,\"Longitude\":-75.57841,\"Description\":null},{\"Latitude\":6.2138300000000006,\"Longitude\":-75.578430000000012,\"Description\":null},{\"Latitude\":6.2137,\"Longitude\":-75.57845,\"Description\":null},{\"Latitude\":6.21356,\"Longitude\":-75.57846,\"Description\":null},{\"Latitude\":6.21339,\"Longitude\":-75.57849,\"Description\":null},{\"Latitude\":6.2132200000000006,\"Longitude\":-75.578520000000012,\"Description\":null},{\"Latitude\":6.2132200000000006,\"Longitude\":-75.578520000000012,\"Description\":null},{\"Latitude\":6.2130300000000007,\"Longitude\":-75.57854,\"Description\":null},{\"Latitude\":6.21285,\"Longitude\":-75.57857,\"Description\":null},{\"Latitude\":6.2124000000000006,\"Longitude\":-75.57862,\"Description\":null},{\"Latitude\":6.21194,\"Longitude\":-75.57867,\"Description\":null},{\"Latitude\":6.2119100000000005,\"Longitude\":-75.57868,\"Description\":null},{\"Latitude\":6.2111600000000005,\"Longitude\":-75.5788,\"Description\":null},{\"Latitude\":6.21105,\"Longitude\":-75.57881,\"Description\":null},{\"Latitude\":6.2100300000000006,\"Longitude\":-75.57895,\"Description\":null},{\"Latitude\":6.2091100000000008,\"Longitude\":-75.57908,\"Description\":null},{\"Latitude\":6.2091100000000008,\"Longitude\":-75.57908,\"Description\":null},{\"Latitude\":6.20903,\"Longitude\":-75.579060000000013,\"Description\":null},{\"Latitude\":6.20898,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.20896,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.20894,\"Longitude\":-75.57904,\"Description\":null},{\"Latitude\":6.2089200000000009,\"Longitude\":-75.57904,\"Description\":null},{\"Latitude\":6.2089000000000008,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.2088800000000006,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.2088600000000005,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.20884,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.2088,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.2087600000000007,\"Longitude\":-75.579060000000013,\"Description\":null},{\"Latitude\":6.20875,\"Longitude\":-75.579060000000013,\"Description\":null},{\"Latitude\":6.20873,\"Longitude\":-75.579060000000013,\"Description\":null},{\"Latitude\":6.2087200000000005,\"Longitude\":-75.579060000000013,\"Description\":null},{\"Latitude\":6.2087100000000008,\"Longitude\":-75.579050000000009,\"Description\":null},{\"Latitude\":6.20868,\"Longitude\":-75.57904,\"Description\":null},{\"Latitude\":6.20868,\"Longitude\":-75.57904,\"Description\":null},{\"Latitude\":6.2080300000000008,\"Longitude\":-75.57911,\"Description\":null},{\"Latitude\":6.2073800000000006,\"Longitude\":-75.57921,\"Description\":null},{\"Latitude\":6.2061600000000006,\"Longitude\":-75.5794,\"Description\":null},{\"Latitude\":6.2049300000000009,\"Longitude\":-75.5796,\"Description\":null},{\"Latitude\":6.2036000000000007,\"Longitude\":-75.579810000000009,\"Description\":null},{\"Latitude\":6.20298,\"Longitude\":-75.579910000000012,\"Description\":null},{\"Latitude\":6.2011600000000007,\"Longitude\":-75.580120000000008,\"Description\":null},{\"Latitude\":6.1996,\"Longitude\":-75.58035000000001,\"Description\":null},{\"Latitude\":6.19868,\"Longitude\":-75.5806,\"Description\":null},{\"Latitude\":6.19814,\"Longitude\":-75.580790000000007,\"Description\":null},{\"Latitude\":6.19761,\"Longitude\":-75.580980000000011,\"Description\":null},{\"Latitude\":6.197,\"Longitude\":-75.581210000000013,\"Description\":null},{\"Latitude\":6.19638,\"Longitude\":-75.58145,\"Description\":null},{\"Latitude\":6.1959000000000009,\"Longitude\":-75.58162,\"Description\":null},{\"Latitude\":6.1956700000000007,\"Longitude\":-75.58171,\"Description\":null},{\"Latitude\":6.19542,\"Longitude\":-75.5818,\"Description\":null},{\"Latitude\":6.1953600000000009,\"Longitude\":-75.581830000000011,\"Description\":null},{\"Latitude\":6.1953000000000005,\"Longitude\":-75.58185,\"Description\":null},{\"Latitude\":6.19474,\"Longitude\":-75.582060000000013,\"Description\":null},{\"Latitude\":6.1931,\"Longitude\":-75.58253,\"Description\":null},{\"Latitude\":6.19111,\"Longitude\":-75.582990000000009,\"Description\":null},{\"Latitude\":6.1907600000000009,\"Longitude\":-75.58306,\"Description\":null},{\"Latitude\":6.19003,\"Longitude\":-75.58323,\"Description\":null},{\"Latitude\":6.1897,\"Longitude\":-75.58332,\"Description\":null},{\"Latitude\":6.1895700000000007,\"Longitude\":-75.583360000000013,\"Description\":null},{\"Latitude\":6.18939,\"Longitude\":-75.58342,\"Description\":null},{\"Latitude\":6.1888200000000007,\"Longitude\":-75.583700000000007,\"Description\":null},{\"Latitude\":6.18862,\"Longitude\":-75.58383,\"Description\":null},{\"Latitude\":6.18836,\"Longitude\":-75.58402000000001,\"Description\":null},{\"Latitude\":6.18801,\"Longitude\":-75.58432,\"Description\":null},{\"Latitude\":6.18773,\"Longitude\":-75.58463,\"Description\":null},{\"Latitude\":6.1868300000000005,\"Longitude\":-75.58578,\"Description\":null},{\"Latitude\":6.1865200000000007,\"Longitude\":-75.58624,\"Description\":null},{\"Latitude\":6.18587,\"Longitude\":-75.58709,\"Description\":null},{\"Latitude\":6.1855800000000007,\"Longitude\":-75.58747000000001,\"Description\":null},{\"Latitude\":6.1850000000000005,\"Longitude\":-75.58814000000001,\"Description\":null},{\"Latitude\":6.1848,\"Longitude\":-75.58834,\"Description\":null},{\"Latitude\":6.1842500000000005,\"Longitude\":-75.588850000000008,\"Description\":null},{\"Latitude\":6.1840500000000009,\"Longitude\":-75.58901,\"Description\":null},{\"Latitude\":6.18362,\"Longitude\":-75.58934,\"Description\":null},{\"Latitude\":6.1831900000000006,\"Longitude\":-75.58965,\"Description\":null},{\"Latitude\":6.18226,\"Longitude\":-75.59029000000001,\"Description\":null},{\"Latitude\":6.18193,\"Longitude\":-75.590520000000012,\"Description\":null},{\"Latitude\":6.18163,\"Longitude\":-75.590700000000012,\"Description\":null},{\"Latitude\":6.18088,\"Longitude\":-75.591100000000012,\"Description\":null},{\"Latitude\":6.1795000000000009,\"Longitude\":-75.59202,\"Description\":null},{\"Latitude\":6.17909,\"Longitude\":-75.59229,\"Description\":null},{\"Latitude\":6.17909,\"Longitude\":-75.59229,\"Description\":null},{\"Latitude\":6.1788000000000007,\"Longitude\":-75.592490000000012,\"Description\":null},{\"Latitude\":6.17851,\"Longitude\":-75.59269,\"Description\":null},{\"Latitude\":6.17818,\"Longitude\":-75.59295,\"Description\":null},{\"Latitude\":6.1778600000000008,\"Longitude\":-75.593240000000009,\"Description\":null},{\"Latitude\":6.1775400000000005,\"Longitude\":-75.593550000000008,\"Description\":null},{\"Latitude\":6.1773200000000008,\"Longitude\":-75.59377,\"Description\":null},{\"Latitude\":6.17694,\"Longitude\":-75.59421,\"Description\":null},{\"Latitude\":6.17689,\"Longitude\":-75.594270000000009,\"Description\":null},{\"Latitude\":6.1765700000000008,\"Longitude\":-75.594680000000011,\"Description\":null},{\"Latitude\":6.17567,\"Longitude\":-75.595940000000013,\"Description\":null},{\"Latitude\":6.1756400000000005,\"Longitude\":-75.595980000000012,\"Description\":null},{\"Latitude\":6.1756,\"Longitude\":-75.59602000000001,\"Description\":null},{\"Latitude\":6.1745500000000009,\"Longitude\":-75.59751,\"Description\":null},{\"Latitude\":6.1741100000000007,\"Longitude\":-75.598120000000009,\"Description\":null},{\"Latitude\":6.1736200000000006,\"Longitude\":-75.59883,\"Description\":null},{\"Latitude\":6.17328,\"Longitude\":-75.599330000000009,\"Description\":null},{\"Latitude\":6.17267,\"Longitude\":-75.60011,\"Description\":null},{\"Latitude\":6.17227,\"Longitude\":-75.6007,\"Description\":null},{\"Latitude\":6.17217,\"Longitude\":-75.600850000000008,\"Description\":null},{\"Latitude\":6.1721100000000009,\"Longitude\":-75.60092,\"Description\":null},{\"Latitude\":6.17206,\"Longitude\":-75.60097,\"Description\":null},{\"Latitude\":6.1717600000000008,\"Longitude\":-75.60136,\"Description\":null},{\"Latitude\":6.1714800000000007,\"Longitude\":-75.60175000000001,\"Description\":null},{\"Latitude\":6.1712200000000008,\"Longitude\":-75.60209,\"Description\":null},{\"Latitude\":6.1707800000000006,\"Longitude\":-75.602700000000013,\"Description\":null},{\"Latitude\":6.1702100000000009,\"Longitude\":-75.603550000000013,\"Description\":null},{\"Latitude\":6.1691,\"Longitude\":-75.60508,\"Description\":null},{\"Latitude\":6.1689900000000009,\"Longitude\":-75.605240000000009,\"Description\":null},{\"Latitude\":6.16887,\"Longitude\":-75.6054,\"Description\":null},{\"Latitude\":6.16863,\"Longitude\":-75.60575,\"Description\":null},{\"Latitude\":6.1684500000000009,\"Longitude\":-75.60602,\"Description\":null},{\"Latitude\":6.1684,\"Longitude\":-75.60611,\"Description\":null},{\"Latitude\":6.16823,\"Longitude\":-75.60633,\"Description\":null},{\"Latitude\":6.1680600000000005,\"Longitude\":-75.606550000000013,\"Description\":null},{\"Latitude\":6.1678900000000008,\"Longitude\":-75.60678,\"Description\":null},{\"Latitude\":6.1677000000000008,\"Longitude\":-75.60701,\"Description\":null},{\"Latitude\":6.16736,\"Longitude\":-75.60748000000001,\"Description\":null},{\"Latitude\":6.16736,\"Longitude\":-75.60748000000001,\"Description\":null},{\"Latitude\":6.1672800000000008,\"Longitude\":-75.607430000000008,\"Description\":null},{\"Latitude\":6.1669500000000008,\"Longitude\":-75.607250000000008,\"Description\":null},{\"Latitude\":6.1668800000000008,\"Longitude\":-75.6072,\"Description\":null},{\"Latitude\":6.1668800000000008,\"Longitude\":-75.6072,\"Description\":null},{\"Latitude\":6.1668,\"Longitude\":-75.607210000000009,\"Description\":null},{\"Latitude\":6.1667600000000009,\"Longitude\":-75.607210000000009,\"Description\":null},{\"Latitude\":6.1667200000000006,\"Longitude\":-75.6072,\"Description\":null},{\"Latitude\":6.16671,\"Longitude\":-75.6072,\"Description\":null},{\"Latitude\":6.16668,\"Longitude\":-75.60719,\"Description\":null},{\"Latitude\":6.1666500000000006,\"Longitude\":-75.60718,\"Description\":null},{\"Latitude\":6.16661,\"Longitude\":-75.60715,\"Description\":null},{\"Latitude\":6.16631,\"Longitude\":-75.606980000000007,\"Description\":null},{\"Latitude\":6.1658800000000005,\"Longitude\":-75.60674,\"Description\":null},{\"Latitude\":6.1654500000000008,\"Longitude\":-75.606500000000011,\"Description\":null},{\"Latitude\":6.1651700000000007,\"Longitude\":-75.60634,\"Description\":null},{\"Latitude\":6.16488,\"Longitude\":-75.606190000000012,\"Description\":null},{\"Latitude\":6.1648600000000009,\"Longitude\":-75.60617,\"Description\":null},{\"Latitude\":6.16483,\"Longitude\":-75.60616,\"Description\":null},{\"Latitude\":6.1642100000000006,\"Longitude\":-75.60581,\"Description\":null},{\"Latitude\":6.1638100000000007,\"Longitude\":-75.60559,\"Description\":null},{\"Latitude\":6.1635800000000005,\"Longitude\":-75.605460000000008,\"Description\":null},{\"Latitude\":6.1634400000000005,\"Longitude\":-75.605380000000011,\"Description\":null},{\"Latitude\":6.16335,\"Longitude\":-75.60532,\"Description\":null},{\"Latitude\":6.16312,\"Longitude\":-75.60514,\"Description\":null},{\"Latitude\":6.16298,\"Longitude\":-75.604970000000009,\"Description\":null},{\"Latitude\":6.16286,\"Longitude\":-75.60486,\"Description\":null},{\"Latitude\":6.1627300000000007,\"Longitude\":-75.60474,\"Description\":null},{\"Latitude\":6.1624000000000008,\"Longitude\":-75.60441,\"Description\":null},{\"Latitude\":6.1624000000000008,\"Longitude\":-75.60441,\"Description\":null},{\"Latitude\":6.1621500000000005,\"Longitude\":-75.604340000000008,\"Description\":null},{\"Latitude\":6.1619600000000005,\"Longitude\":-75.60433,\"Description\":null},{\"Latitude\":6.16173,\"Longitude\":-75.60437,\"Description\":null},{\"Latitude\":6.1614700000000004,\"Longitude\":-75.604490000000013,\"Description\":null},{\"Latitude\":6.1614,\"Longitude\":-75.604530000000011,\"Description\":null},{\"Latitude\":6.16136,\"Longitude\":-75.60456,\"Description\":null},{\"Latitude\":6.1612800000000005,\"Longitude\":-75.6046,\"Description\":null},{\"Latitude\":6.1612800000000005,\"Longitude\":-75.6046,\"Description\":null},{\"Latitude\":6.1610900000000006,\"Longitude\":-75.60483,\"Description\":null},{\"Latitude\":6.16079,\"Longitude\":-75.605250000000012,\"Description\":null},{\"Latitude\":6.16079,\"Longitude\":-75.605250000000012,\"Description\":null},{\"Latitude\":6.1606900000000007,\"Longitude\":-75.60539,\"Description\":null},{\"Latitude\":6.16058,\"Longitude\":-75.60553,\"Description\":null},{\"Latitude\":6.1604500000000009,\"Longitude\":-75.605730000000008,\"Description\":null},{\"Latitude\":6.16042,\"Longitude\":-75.60577,\"Description\":null},{\"Latitude\":6.1602900000000007,\"Longitude\":-75.60597,\"Description\":null},{\"Latitude\":6.1600500000000009,\"Longitude\":-75.606310000000008,\"Description\":null},{\"Latitude\":6.1598200000000007,\"Longitude\":-75.60665,\"Description\":null},{\"Latitude\":6.1596600000000006,\"Longitude\":-75.60688,\"Description\":null},{\"Latitude\":6.1595,\"Longitude\":-75.60711,\"Description\":null},{\"Latitude\":6.1593000000000009,\"Longitude\":-75.60742,\"Description\":null},{\"Latitude\":6.1588600000000007,\"Longitude\":-75.60802000000001,\"Description\":null},{\"Latitude\":6.15845,\"Longitude\":-75.608640000000008,\"Description\":null},{\"Latitude\":6.1584,\"Longitude\":-75.60872,\"Description\":null},{\"Latitude\":6.1583600000000009,\"Longitude\":-75.608790000000013,\"Description\":null},{\"Latitude\":6.15831,\"Longitude\":-75.60887000000001,\"Description\":null},{\"Latitude\":6.1582300000000005,\"Longitude\":-75.60902,\"Description\":null},{\"Latitude\":6.1580600000000008,\"Longitude\":-75.609220000000008,\"Description\":null},{\"Latitude\":6.1579000000000006,\"Longitude\":-75.60939,\"Description\":null},{\"Latitude\":6.15779,\"Longitude\":-75.60953,\"Description\":null},{\"Latitude\":6.1577100000000007,\"Longitude\":-75.609680000000012,\"Description\":null},{\"Latitude\":6.1575900000000008,\"Longitude\":-75.60988,\"Description\":null},{\"Latitude\":6.1569800000000008,\"Longitude\":-75.61078,\"Description\":null},{\"Latitude\":6.1563200000000009,\"Longitude\":-75.611740000000012,\"Description\":null},{\"Latitude\":6.15613,\"Longitude\":-75.61203,\"Description\":null},{\"Latitude\":6.15596,\"Longitude\":-75.61226,\"Description\":null},{\"Latitude\":6.1559000000000008,\"Longitude\":-75.61236000000001,\"Description\":null},{\"Latitude\":6.1559000000000008,\"Longitude\":-75.61236000000001,\"Description\":null},{\"Latitude\":6.15575,\"Longitude\":-75.61233,\"Description\":null},{\"Latitude\":6.1556000000000006,\"Longitude\":-75.612130000000008,\"Description\":null},{\"Latitude\":6.15542,\"Longitude\":-75.61187000000001,\"Description\":null},{\"Latitude\":6.15542,\"Longitude\":-75.61187000000001,\"Description\":null},{\"Latitude\":6.15509,\"Longitude\":-75.612050000000011,\"Description\":null},{\"Latitude\":6.1550400000000005,\"Longitude\":-75.61207,\"Description\":null},{\"Latitude\":6.15502,\"Longitude\":-75.612090000000009,\"Description\":null},{\"Latitude\":6.155,\"Longitude\":-75.612100000000012,\"Description\":null},{\"Latitude\":6.1549900000000006,\"Longitude\":-75.61212,\"Description\":null},{\"Latitude\":6.1549700000000005,\"Longitude\":-75.61215,\"Description\":null},{\"Latitude\":6.1549600000000009,\"Longitude\":-75.61217,\"Description\":null},{\"Latitude\":6.1548200000000008,\"Longitude\":-75.61257,\"Description\":null},{\"Latitude\":6.1547100000000006,\"Longitude\":-75.61291,\"Description\":null},{\"Latitude\":6.1545900000000007,\"Longitude\":-75.613260000000011,\"Description\":null},{\"Latitude\":6.1545900000000007,\"Longitude\":-75.613260000000011,\"Description\":null},{\"Latitude\":6.1545200000000007,\"Longitude\":-75.61323,\"Description\":null},{\"Latitude\":6.15444,\"Longitude\":-75.613210000000009,\"Description\":null},{\"Latitude\":6.15437,\"Longitude\":-75.61318,\"Description\":null},{\"Latitude\":6.1543,\"Longitude\":-75.61315,\"Description\":null},{\"Latitude\":6.1542,\"Longitude\":-75.613120000000009,\"Description\":null},{\"Latitude\":6.15411,\"Longitude\":-75.61309,\"Description\":null},{\"Latitude\":6.15411,\"Longitude\":-75.61309,\"Description\":null},{\"Latitude\":6.1539100000000007,\"Longitude\":-75.61330000000001,\"Description\":null},{\"Latitude\":6.15385,\"Longitude\":-75.61336,\"Description\":null},{\"Latitude\":6.1537200000000007,\"Longitude\":-75.61351,\"Description\":null},{\"Latitude\":6.15357,\"Longitude\":-75.613670000000013,\"Description\":null},{\"Latitude\":6.15347,\"Longitude\":-75.61378,\"Description\":null},{\"Latitude\":6.1532300000000006,\"Longitude\":-75.61405,\"Description\":null},{\"Latitude\":6.1528800000000006,\"Longitude\":-75.614430000000013,\"Description\":null},{\"Latitude\":6.1525200000000009,\"Longitude\":-75.61481,\"Description\":null},{\"Latitude\":6.1522400000000008,\"Longitude\":-75.61511,\"Description\":null},{\"Latitude\":6.1521300000000005,\"Longitude\":-75.61524,\"Description\":null},{\"Latitude\":6.1519400000000006,\"Longitude\":-75.61544,\"Description\":null},{\"Latitude\":6.1517800000000005,\"Longitude\":-75.615640000000013,\"Description\":null}]";
    }
}