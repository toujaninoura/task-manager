using FluentValidation;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Validators;

public class InviteCollaboratorValidator : AbstractValidator<InviteCollaboratorRequest>
{
    public InviteCollaboratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Role)
            .Must(r => r == TaskShareRole.Editor || r == TaskShareRole.Viewer)
            .WithMessage("Role must be Editor or Viewer. Owner cannot be assigned.");
    }
}
