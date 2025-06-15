using Domain.DTO;
using FluentValidation;

namespace Domain.Validators;

public class ActualizarUsuarioDtoValidator : AbstractValidator<ActualizarUsuarioDto>
{
    public ActualizarUsuarioDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Bio)
            .MaximumLength(160);
        RuleFor(x => x.AvatarUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .WithMessage("AvatarUrl debe ser una URL válida.");
    }
}