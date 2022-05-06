using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        public List<string> CardTypes { get; } = new() { "Visa", "Master Card", "American Express" };

        public List<string> PaymentStatuses { get; } = new() { "Accepted", "Rejected" };

        [HttpPost]
        [Route("performpayment")]
        public IEnumerable<string> PerformPayment(int userId, string reference)
        {
            // just to add some latency
            Thread.Sleep(500);

            // let's say that based on the user identification the payment system is able to retrieve the user payment information.
            // the payment system returns the response in a list of string like this: payment status, card type, card number, user and reference
            return new[]
            {
                PaymentStatuses[new Random().Next(0, 2)],
                CardTypes[new Random().Next(0, 3)],
                Guid.NewGuid().ToString(),
                userId.ToString(),
                reference
            };
        }
    }
}
