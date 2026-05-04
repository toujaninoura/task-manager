using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class UpdateTaskItemValidator : AbstractValidator<UpdateTaskItemRequest>
{
    public UpdateTaskItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
