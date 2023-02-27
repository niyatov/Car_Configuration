using Car_Configuration.Dtoes;
using FluentValidation;

namespace Car_Configuration.Validations.Wheel;

public class UpdateWheelDtoValidator : AbstractValidator<UpdateWheelDto>
{
    public UpdateWheelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Name is required")
            .MinimumLength(3)
            .WithMessage("Name should be at least 3 characters long")
            .MaximumLength(30)
            .WithMessage("Name should not exceed 50 characters");
    }
}
