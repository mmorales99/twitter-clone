using Domain.DTO;
using FluentValidation;

namespace Domain.Validators;

public class ActualizarMensajeDtoValidator : AbstractValidator<ActualizarMensajeDto>
{
    public ActualizarMensajeDtoValidator()
    {
        RuleFor(x => x.Contenido)
            .NotEmpty()
            .MaximumLength(280);
    }
}