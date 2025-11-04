using FluentValidation;
using FluentValidation.Results;

namespace Identidade.Dominio.Helpers
{
    public interface IPasswordValidator
    {
        ValidationResult Validate(string password);
    }

    public class PasswordValidator : AbstractValidator<string>, IPasswordValidator
    {
        public PasswordValidator()
        {
            RuleFor(p => p)
                .MinimumLength(6)
                .OnFailure(_ => throw new AppException("The password should contain at least 6 characters."));
        }

        public new ValidationResult Validate(string password)
        {
            if (password == null)
                throw new AppException("The password can not be null.");

            return base.Validate(password);
        }
    }
}
