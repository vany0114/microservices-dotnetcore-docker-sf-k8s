using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Driver.Model;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Model;
using Duber.Domain.User.Repository;
using Microsoft.AspNetCore.Mvc;
using Duber.WebSite.Models;

namespace Duber.WebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IDriverRepository _driverRepository;

        public HomeController(IUserRepository userRepository, IDriverRepository driverRepository)
        {
            _userRepository = userRepository;
            _driverRepository = driverRepository;
        }

        public IActionResult Index()
        {
            // users test
            //var users = _userRepository.GetUsersAsync().Result;
            //var user = users[0];
            //user.ChangePaymentMethod(PaymentMethod.Cash);
            //_userRepository.Update(user);
            //var result =_userRepository.UnitOfWork.SaveChangesAsync().Result;
            //var paymentMethod = user.PaymentMethod;

            // drivers test
            //var drivers = _driverRepository.GetDriversAsync().Result;
            //var driver = drivers[0];
            //driver.AddVehicle("TWN 741", "Lexus", "2018", VehicleType.Car);
            //_driverRepository.Update(driver);
            //var result = _driverRepository.UnitOfWork.SaveChangesAsync().Result;
            //var currentVehicle = driver.CurrentVehicle;

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
