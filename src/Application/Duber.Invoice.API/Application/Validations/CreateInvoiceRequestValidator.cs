using Duber.Invoice.API.Application.Model;
using FluentValidation;

namespace Duber.Invoice.API.Application.Validations
{
    public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
    {
        public CreateInvoiceRequestValidator()
        {
            RuleFor(request => request.TripId).NotEmpty().WithMessage("Trip id is required.");
            RuleFor(request => request.UserId).NotEmpty().WithMessage("User id is required.");
        }
    }
}
