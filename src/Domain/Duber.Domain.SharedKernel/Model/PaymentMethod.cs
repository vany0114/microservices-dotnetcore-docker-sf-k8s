using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Infrastructure.DDD;

namespace Duber.Domain.SharedKernel.Model
{
    public class PaymentMethod : Enumeration
    {
        public static PaymentMethod Cash = new PaymentMethod(1, "Cash");
        public static PaymentMethod CreditCard = new PaymentMethod(2, "Credit Card");

        protected PaymentMethod() { }

        public PaymentMethod(int id, string name)
            : base(id, name)
        {

        }

        public static IEnumerable<PaymentMethod> List()
        {
            return new[] { Cash, CreditCard };
        }

        public static PaymentMethod FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for PaymentMethod: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static PaymentMethod From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for PaymentMethod: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}
