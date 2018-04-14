using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Driver.Model;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Model;
using Duber.Domain.User.Repository;
using Duber.WebSite.Extensions;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Duber.WebSite.Controllers
{
    public class TripController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly Dictionary<SelectListItem, LocationModel> _originsAndDestinations;

        public TripController(IUserRepository userRepository, IDriverRepository driverRepository, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _driverRepository = driverRepository;
            _cache = cache;

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

        public async Task<IActionResult> SimulateTrip(TripRequestModel model)
        {
            var drivers = await GetDrivers();
            var users = await GetUsers();
            model.Drivers = drivers.ToSelectList();
            model.Users = users.ToSelectList();
            model.Origins = _originsAndDestinations.Keys.Select(x => x).ToList();
            model.Destinations = _originsAndDestinations.Keys.Select(x => x).ToList();
            model.Places = _originsAndDestinations.Values.Select(x => x).ToList();

            if (ModelState.IsValid)
            {
                await Task.Delay(1);
            }

            return View("Index", model);
        }

        public IActionResult TripsByUser()
        {
            return View();
        }

        public IActionResult TripsByDriver()
        {
            return View();
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
    }
}