using Domain.DTO;
using FluentValidation;

namespace Domain.Validators;

public class CrearMensajeDtoValidator : AbstractValidator<CrearMensajeDto>
{
    public CrearMensajeDtoValidator()
    {
        RuleFor(x => x.Contenido)
            .NotEmpty()
            .MaximumLength(280);
        RuleFor(x => x.ReplyTo)
            .Matches("^[a-fA-F0-9]{24}$")
            .When(x => !string.IsNullOrEmpty(x.ReplyTo))
            .WithMessage("ReplyTo debe ser un id válido de MongoDB.");
    }
}
