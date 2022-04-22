using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly List<string> _paymentStatuses = new List<string> { "Accepted", "Rejected" };
        private readonly List<string> _cardTypes = new List<string> { "Visa", "Master Card", "American Express" };

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
                _paymentStatuses[new Random().Next(0, 2)],
                _cardTypes[new Random().Next(0, 3)],
                Guid.NewGuid().ToString(),
                userId.ToString(),
                reference
            };
        }
    }
}
