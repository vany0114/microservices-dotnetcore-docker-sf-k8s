using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Domain.SharedKernel.Model;
using Duber.Domain.User.Exceptions;
using Duber.Infrastructure.DDD;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Duber.Domain.User.Model
{
    public class User : Entity, IAggregateRoot
    {
        private string _name;
        private string _numberPhone;
        private string _email;
        private int _rating;
        private int _paymentMethodId;

        public string Name => _name;

        public string Email => _email;

        public string NumberPhone => _numberPhone;

        public int Rating => _rating;

        // EF navigation property
        public PaymentMethod PaymentMethod { get; private set; }

        protected User()
        {
        }

        public User(string name, string email, int rating, PaymentMethod paymentMethod, string numberPhone = null)
        {
            if (rating < 0 || rating > 5) throw new UserDomainException("User rating should be between 1 and 5");

            _name = !string.IsNullOrWhiteSpace(name) ? name : throw new UserDomainException(nameof(name));
            _email = !string.IsNullOrWhiteSpace(email) ? email : throw new UserDomainException(nameof(email));
            _numberPhone = numberPhone;
            _rating = rating;
            _paymentMethodId = paymentMethod.Id;
        }

        public void CalculateRating(List<int> historicRatings)
        {
            // this is just an example, to show that here is where you perform the business validations and define the object behavior.
            // note: historicRatings shouldn't be a parameter, just to example purposes. It should be a value object (list) inside of this aggregate
            if (historicRatings.Count == 0)
            {
                _rating = 0;
            }
            else
            {
                _rating = historicRatings.Sum() / historicRatings.Count;
            }
        }

        public void ChangePaymentMethod(PaymentMethod newMethod)
        {
            if (_paymentMethodId == newMethod.Id)
                throw new InvalidOperationException($"The user already has the {PaymentMethod.Name} payment method.");

            _paymentMethodId = newMethod.Id;
        }
    }
}
