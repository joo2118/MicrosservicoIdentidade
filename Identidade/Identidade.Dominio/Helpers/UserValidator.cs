using FluentValidation;
using FluentValidation.Results;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identidade.Dominio.Helpers
{
    public interface IUserValidator
    {
        ValidationResult Validate(string userName);
        void VerifyExistences(IEnumerable<string> usersIds, IEnumerable<string> userGroupsIds);
    }

    public class UserValidator : AbstractValidator<string>, IUserValidator
    {
        private readonly IReadOnlyRepository<User> _userRepository;
        private readonly IReadOnlyRepository<UserGroup> _userGroupReadOnlyRepository;

        public UserValidator(IReadOnlyRepository<User> userRepository, IUserGroupRepository userGroupRepository)
        {
            _userRepository = userRepository;
            _userGroupReadOnlyRepository = userGroupRepository as IReadOnlyRepository<UserGroup>;

            if (_userGroupReadOnlyRepository is null)
                throw new ArgumentException($"{nameof(userGroupRepository)} must implement {nameof(IReadOnlyRepository<UserGroup>)} to support batched lookups.", nameof(userGroupRepository));

            RuleFor(userName => userName)
                .Must(userName => !string.IsNullOrWhiteSpace(userName))
                .OnFailure(_ => throw new AppException("The username can not be empty."));

            RuleFor(userName => userName)
                .MustAsync(VerifyUserInexistence)
                .OnFailure(_ => throw new ConflictAppException("There already exists a user with the same username."));
        }

        public new ValidationResult Validate(string userName)
        {
            if (userName == null)
                throw new AppException("The username can not be null.");

            return base.Validate(userName);
        }

        public void VerifyExistences(IEnumerable<string> usersIds, IEnumerable<string> userGroupsIds)
        {
            VerifyUsersExistences(usersIds);
            VerifyUserGroupExistences(userGroupsIds);
        }

        private void VerifyUserGroupExistences(IEnumerable<string> userGroupIds)
        {
            if (userGroupIds is null || !userGroupIds.Any())
                return;

            var ids = userGroupIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
            if (ids.Length == 0)
                return;

            var userGroupsFoundById = _userGroupReadOnlyRepository.GetByIds(ids).GetAwaiter().GetResult();
            var missingIds = ids
                .Where(id => !userGroupsFoundById.ContainsKey(id))
                .ToArray();

            if (missingIds.Length > 0)
                throw new NotFoundAppException("User Groups", "ID", string.Join(", ", missingIds));
        }

        private void VerifyUsersExistences(IEnumerable<string> usersIds)
        {
            if (usersIds is null || !usersIds.Any())
                return;

            var ids = usersIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
            if (ids.Length == 0)
                return;

            var usersFoundById = _userRepository.GetByIds(ids).GetAwaiter().GetResult();
            var missingIds = ids
                .Where(id => !usersFoundById.ContainsKey(id))
                .ToArray();

            if (missingIds.Length > 0)
                throw new NotFoundAppException("Users", "ID", string.Join(", ", missingIds));
        }

        private async Task<bool> VerifyUserInexistence(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _userRepository.GetByName(userName);
                return false;
            }
            catch (NotFoundAppException)
            {
                return true;
            }
        }
    }
}
