using System;
using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty
#pragma warning disable 649

namespace Duber.Domain.SharedKernel.Model
{
    public class PaymentInfo : ValueObject
    {
        private PaymentStatus _status;
        private Guid _invoiceId;
        private int _userId;
        private string _cardNumber;
        private string _cardType;

        public PaymentInfo(int userId, PaymentStatus status, string cardNumber, string cardType)
        {
            if (userId == default(int)) throw new ArgumentNullException(nameof(userId));

            _userId = userId;
            _status = status;
            _cardNumber = cardNumber ?? throw new ArgumentNullException(nameof(cardNumber));
            _cardType = cardType ?? throw new ArgumentNullException(nameof(cardType));
        }

        // Just to EF creates the one to one relationship. (need only in the migrations)
        public Guid InvoiceId => _invoiceId;

        public int UserId => _userId;

        public PaymentStatus Status => _status;

        public string CardNumber => _cardNumber;

        public string CardType => _cardType;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return UserId;
            yield return Status;
            yield return CardNumber;
            yield return CardType;
            yield return InvoiceId;
        }
    }

    public enum PaymentStatus
    {
        Accepted = 1,
        Rejected = 2,
    }
}
