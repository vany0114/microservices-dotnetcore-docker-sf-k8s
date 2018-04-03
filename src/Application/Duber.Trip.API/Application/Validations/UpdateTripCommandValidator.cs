using Duber.Trip.API.Application.Model;
using FluentValidation;

namespace Duber.Trip.API.Application.Validations
{
    public class UpdateTripCommandValidator : AbstractValidator<UpdateTripCommand>
    {
        public UpdateTripCommandValidator()
        {
            RuleFor(trip => trip.Id).NotEmpty().WithMessage("Trip id is required.");
        }
    }
}
