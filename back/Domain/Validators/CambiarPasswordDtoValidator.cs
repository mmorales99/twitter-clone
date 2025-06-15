using Domain.DTO;
using FluentValidation;

namespace Domain.Validators;

public class CambiarPasswordDtoValidator : AbstractValidator<CambiarPasswordDto>
{
    public CambiarPasswordDtoValidator()
    {
        RuleFor(x => x.PasswordNueva)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Debe contener al menos una may�scula")
            .Matches("[a-z]").WithMessage("Debe contener al menos una min�scula")
            .Matches("[0-9]").WithMessage("Debe contener al menos un n�mero")
            .Matches("[^a-zA-Z0-9]").WithMessage("Debe contener al menos un s�mbolo");
    }
}
