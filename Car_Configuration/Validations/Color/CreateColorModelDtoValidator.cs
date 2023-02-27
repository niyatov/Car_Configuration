using Car_Configuration.Dtoes;
using FluentValidation;

namespace Car_Configuration.Validations.Color;

public class CreateColorModelDtoValidator : AbstractValidator<CreateColorModelDto>
{
    public CreateColorModelDtoValidator()
    {
        RuleFor(x => x.ModelName)
            .NotNull()
            .WithMessage("Name is required")
            .MinimumLength(4)
            .WithMessage("Name should be at least 4 characters long")
            .MaximumLength(30)
            .WithMessage("Name should not exceed 50 characters");

        RuleFor(x => x.ColorName)
            .NotNull()
            .WithMessage("Name is required")
            .MinimumLength(3)
            .WithMessage("Name should be at least 3 characters long")
            .MaximumLength(30)
            .WithMessage("Name should not exceed 50 characters");
    }
}