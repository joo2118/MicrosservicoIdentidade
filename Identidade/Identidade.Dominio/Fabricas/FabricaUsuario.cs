using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;

namespace Identidade.Dominio.Fabricas
{
    public sealed class FabricaUsuario : IFabricaUsuario
    {
        private readonly IReadOnlyRepository<User> _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;

        public FabricaUsuario(IReadOnlyRepository<User> userRepository, IUserGroupRepository userGroupRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userGroupRepository = userGroupRepository ?? throw new ArgumentNullException(nameof(userGroupRepository));
        }

        public async Task<User> MapearParaUsuarioAsync(InputUserDto dto, string originalUserLogin = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var user = await ObterUsuarioOriginalSeNecessarioAsync(originalUserLogin, cancellationToken);
            user ??= new User();

            if (dto.Login != null)
                user.UserName = dto.Login;

            if (dto.Name != null)
                user.Name = dto.Name;

            if (dto.Email != null)
                user.Email = dto.Email;

            if (dto.PasswordExpiration != null)
                user.PasswordExpiration = dto.PasswordExpiration;

            if (dto.PasswordDoesNotExpire != null)
                user.PasswordDoesNotExpire = dto.PasswordDoesNotExpire;

            if (dto.AuthenticationType != null)
                user.AuthenticationType = dto.AuthenticationType.ToString();

            if (dto.Language != null)
                user.Language = dto.Language.ToString();

            if (dto.Password != null)
                user.PasswordHash = dto.Password;

            if (dto.Active is bool activeNotNull)
                user.LockoutEnd = activeNotNull ? DateTimeOffset.MinValue : DateTimeOffset.MaxValue;

            if (dto.UserGroups != null)
                user.UserGroupUsers = await ConstruirUserGroupUsersAsync(user, dto.UserGroups, cancellationToken);

            if (dto.SubstituteUsers != null)
                user.UserSubstitutions = await ConstruirUserSubstitutionsAsync(user, dto.SubstituteUsers, cancellationToken);

            return user;
        }

        public async Task<User> MapearParaUsuarioAsync(ArcUserDto dto, string originalUserLogin = null, CancellationToken cancellationToken = default)
        {
            ValidarArcUserDto(dto);

            var user = await ObterUsuarioOriginalSeNecessarioAsync(originalUserLogin, cancellationToken);
            user ??= new User();

            user.UserName = dto.Login;
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.PasswordExpiration = dto.PasswordExpiration;
            user.PasswordDoesNotExpire = dto.PasswordDoesNotExpire;
            user.AuthenticationType = dto.AuthenticationType.ToString();
            user.Language = dto.Language.ToString();
            user.PasswordHash = dto.Password;

            if (dto.Active is bool activeNotNull)
                user.LockoutEnd = activeNotNull ? DateTimeOffset.MinValue : DateTimeOffset.MaxValue;

            if (dto.UserGroups != null)
                user.UserGroupUsers = await ConstruirUserGroupUsersAsync(user, dto.UserGroups, cancellationToken);

            if (dto.SubstituteUsers != null)
                user.UserSubstitutions = await ConstruirUserSubstitutionsAsync(user, dto.SubstituteUsers, cancellationToken);

            return user;
        }

        public OutputUserDto MapearParaDtoSaidaUsuario(User user)
        {
            if (user is null)
                return null;

            return new OutputUserDto
            {
                Id = user.Id,
                Login = user.UserName,
                Name = user.Name,
                Email = user.Email,
                PasswordExpiration = user.PasswordExpiration,
                PasswordDoesNotExpire = user.PasswordDoesNotExpire,
                Active = user.LockoutEnd == DateTimeOffset.MinValue,
                AuthenticationType = Enum.TryParse<AuthenticationType>(user.AuthenticationType, true, out var auth) ? auth : null,
                Language = Enum.TryParse<Language>(user.Language, true, out var lang) ? lang : null,
                UserGroups = user.UserGroupUsers?.Select(ugu => ugu.UserGroupId).ToArray() ?? Array.Empty<string>(),
                SubstituteUsers = user.UserSubstitutions?.Select(us => us.SubstituteUserId).ToArray() ?? Array.Empty<string>(),
                CreatedAt = user.CreatedAt,
                LastUpdatedAt = user.LastUpdatedAt
            };
        }

        public ArcUser MapearParaArcUser(ArcUserDto dto)
        {
            if (dto is null)
                return null;

            return new ArcUser(
                dto.Login,
                dto.Name,
                dto.Email,
                dto.Password,
                dto.Language.ToString(),
                dto.UserGroups,
                dto.SubstituteUsers,
                dto.AuthenticationType.ToString(),
                dto.PasswordExpiration,
                dto.PasswordDoesNotExpire ?? false,
                dto.Active ?? false);
        }

        public void ValidarArcUserDto(ArcUserDto user)
        {
            if (user == null)
                throw new InvalidOperationException(Constants.Exception.cst_NullUser);

            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ArgumentException(Constants.Exception.cst_InvalidLogin);

            if (user.AuthenticationType == AuthenticationType.AzureAD && string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException($"Email must be provided for authentication type {AuthenticationType.AzureAD}");

            if ((user.AuthenticationType == AuthenticationType.AzureAD || user.AuthenticationType == AuthenticationType.ActiveDirectory) && !string.IsNullOrWhiteSpace(user.Password))
                throw new InvalidOperationException($"Password must be empty for authentication type {user.AuthenticationType}");

            if (user.AuthenticationType == AuthenticationType.DatabaseUser && string.IsNullOrWhiteSpace(user.Password))
                throw new InvalidOperationException($"Password must be provided for authentication type {user.AuthenticationType}");
        }

        private async Task<User> ObterUsuarioOriginalSeNecessarioAsync(string originalUserLogin, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(originalUserLogin))
                return null;

            return await ExceptionCatcher.ExecuteSafeAsync<NotFoundAppException, User>(
                () => _userRepository.GetByName(originalUserLogin),
                _ => Task.CompletedTask);
        }

        private async Task<ICollection<UserGroupUser>> ConstruirUserGroupUsersAsync(User user, string[] userGroupIds, CancellationToken cancellationToken)
        {
            if (userGroupIds is null || userGroupIds.Length == 0)
                return Array.Empty<UserGroupUser>();

            var ids = userGroupIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (ids.Length == 0)
                return Array.Empty<UserGroupUser>();

            var userGroups = await _userGroupRepository.GetUserGroups(ids);

            // Single pass allocation.
            var result = new List<UserGroupUser>(userGroups.Length);
            foreach (var group in userGroups)
                result.Add(new UserGroupUser(group, user));

            return result;
        }

        private async Task<ICollection<UserSubstitution>> ConstruirUserSubstitutionsAsync(User user, string[] substituteUserIds, CancellationToken cancellationToken)
        {
            if (substituteUserIds is null || substituteUserIds.Length == 0)
                return Array.Empty<UserSubstitution>();

            var ids = substituteUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (ids.Length == 0)
                return Array.Empty<UserSubstitution>();

            var substituteUsers = await _userGroupRepository.GetUsers(ids);

            var result = new List<UserSubstitution>(substituteUsers.Length);
            foreach (var substituteUser in substituteUsers)
                result.Add(new UserSubstitution(user, substituteUser));

            return result;
        }
    }
}
