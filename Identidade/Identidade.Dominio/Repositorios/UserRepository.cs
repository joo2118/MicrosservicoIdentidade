using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Dominio.Repositorios
{
    public interface IUserRepository : IReadOnlyRepository<User>
    {
        Task<User> Create(User user, string password);
        Task<User> Update(User user, string password);
        Task<bool> Remove(string userId);
    }

    public class UserRepository : UserReadOnlyRepository, IUserRepository
    {
        private readonly IUserValidator _userValidator;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IIdGenerator _idGenerator;

        public UserRepository(IARCDbContext arcDbContext, IUserValidator userValidator, IPasswordValidator passwordValidator,
            IUpdateConcurrencyResolver updateConcurrencyResolver, IIdGenerator idGenerator)
            : base(arcDbContext, updateConcurrencyResolver)
        {
            _userValidator = userValidator;
            _passwordValidator = passwordValidator;
            _idGenerator = idGenerator;
        }

        public async Task<User> Create(User user, string password)
        {
            if (user == null)
                throw new AppException(Constants.Exception.cst_NullUser);

            _userValidator.Validate(user.UserName);

            if (password != null)
            {
                _passwordValidator.Validate(password);
                user.PasswordHistory = user.PasswordHash ?? string.Empty;
            }

            user.CreatedAt = DateTime.UtcNow;
            user.LastUpdatedAt = user.CreatedAt;

            user.Id = _idGenerator.GenerateId(Constants.cst_Usr, user.Id);

            foreach (var userSubstitution in user.UserSubstitutions)
            {
                userSubstitution.User = user;
                userSubstitution.UserId = user.Id;
            }

            foreach (var userGroupUser in user.UserGroupUsers)
            {
                userGroupUser.User = user;
                userGroupUser.UserId = user.Id;
            }

            var entityEntry = await _arcDbContext.AddAsync(user);
            await SaveChanges();

            return entityEntry.Entity;
        }

        public async Task<User> Update(User user, string inputPassword)
        {
            user.PasswordHistory = user.PasswordHistory ?? string.Empty;
            var lastPassword = user.PasswordHistory.Split(';').Last();
            if (!string.IsNullOrWhiteSpace(inputPassword))
            {
                if (string.IsNullOrWhiteSpace(lastPassword))
                {
                    user.PasswordHistory = !string.IsNullOrWhiteSpace(user.PasswordHistory)
                        ? user.PasswordHistory + ';' + user.PasswordHash
                        : user.PasswordHash;
                }
            }

            user.LastUpdatedAt = DateTime.UtcNow;
            var entityEntry = _arcDbContext.Update(user);
            await SaveChanges();

            return entityEntry.Entity;
        }

        public async Task<bool> Remove(string userId)
        {
            var user = await GetById(userId);

            var substitutionsToBeRemoved = _arcDbContext.UserSubstitutions.Where(us => us.UserId == userId);
            _arcDbContext.RemoveRange(substitutionsToBeRemoved);
            _arcDbContext.Remove(user);

            var result = await _arcDbContext.SaveChangesAsync();

            return result != 0;
        }
    }
}
