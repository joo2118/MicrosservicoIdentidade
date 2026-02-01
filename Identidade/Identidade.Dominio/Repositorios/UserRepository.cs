using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
            _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
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

            var now = DateTime.UtcNow;
            user.CreatedAt = now;
            user.LastUpdatedAt = now;

            user.Id = _idGenerator.GenerateId(Constants.cst_Usr, user.Id);

            user.UserSubstitutions ??= [];
            foreach (var userSubstitution in user.UserSubstitutions)
            {
                userSubstitution.User = user;
                userSubstitution.UserId = user.Id;
            }

            user.UserGroupUsers ??= [];
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
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.PasswordHistory ??= string.Empty;

            var lastPassword = user.PasswordHistory.Split(';').LastOrDefault();
            if (!string.IsNullOrWhiteSpace(inputPassword) && string.IsNullOrWhiteSpace(lastPassword))
            {
                user.PasswordHistory = !string.IsNullOrWhiteSpace(user.PasswordHistory)
                    ? user.PasswordHistory + ';' + user.PasswordHash
                    : user.PasswordHash;
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

        public Task<IReadOnlyCollection<User>> GetAll(int? page, int? pageSize) =>
            base.GetAll(page, pageSize);

        Task<IReadOnlyCollection<User>> IReadOnlyRepository<User>.GetAll(int? page, int? pageSize) =>
            GetAll(page, pageSize);

        public async Task<IReadOnlyDictionary<string, User>> GetByIds(string[] ids)
        {
            if (ids is null || ids.Length == 0)
                return new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

            var normalized = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();

            if (normalized.Length == 0)
                return new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

            var users = await _arcDbContext.Users
                .AsNoTracking()
                .AsSplitQuery()
                .Where(u => normalized.Contains(u.Id))
                .Include(u => u.UserGroupUsers)
                .ThenInclude(ugu => ugu.UserGroup)
                .Include(u => u.UserSubstitutions)
                .ThenInclude(usu => usu.SubstituteUser)
                .ToArrayAsync();

            return users.ToDictionary(u => u.Id, u => u, StringComparer.OrdinalIgnoreCase);
        }
    }
}
