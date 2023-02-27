using Car_Configuration.Dtoes;
using FluentValidation;

namespace Car_Configuration.Validations.Model;

public class UpdateModelDtoValidator : AbstractValidator<UpdateModelDto>
{
    public UpdateModelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Name is required")
            .MinimumLength(4)
            .WithMessage("Name should be at least 4 characters long")
            .MaximumLength(30)
            .WithMessage("Name should not exceed 50 characters");
    }
}