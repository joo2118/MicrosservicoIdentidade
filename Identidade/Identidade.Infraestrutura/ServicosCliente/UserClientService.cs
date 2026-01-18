using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using AutoMapper;

using MassTransit;

using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Compatibility;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using Identidade.Publico.Events;
using Identidade.Infraestrutura.Configuracoes;
using Identidade.Dominio.Modelos;

namespace Identidade.Infraestrutura.ClientServices
{
    public interface IUserClientService
    {
        Task AssociateToUserGroup(string userId, string userGroupName, string requestUserId);
        Task<OutputUserDto> Create(InputUserDto inputUserDto, string requestUserId, string suggestedId = null);
        Task<OutputUserDto> Create(ArcUserDto arcUserDto, string requestUserId, string suggestedId = null);
        Task<OutputUserDto> CreateApi(InputUserDto inputUserDto, string requestUserId, string suggestedId = null);
        Task Delete(string userId, string requestUserId);
        Task DeleteApi(string userId, string requestUserId);
        Task DissociateFromUserGroup(string userId, string userGroupName, string requestUserId);
        Task<IReadOnlyCollection<OutputUserDto>> Get(string login);
        Task<OutputUserDto> GetById(string userId);
        OutputUserDto GetById(string userId, out string password);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId);
        Task<OutputUserDto> Update(string userId, InputUserDto inputUserDto, string requestUserId);
        Task<OutputUserDto> Update(string userId, ArcUserDto arcUserDto, string requestUserId);
        Task<OutputUserDto> UpdateApi(string userId, InputUserDto inputUserDto, string requestUserId);
    }

    public class UserClientService : IUserClientService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly IUserValidator _userValidator;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IBus _bus;
        private readonly ISettings _settings;
        private readonly IDatabaseConnectionUserModifier _databaseConnectionModifier;
        private readonly IArcUserXmlWriter _arcUserXmlWriter;

        public UserClientService(IUserRepository userRepository, IAuthorizationService authorizationService, IMapper mapper,
            IUserValidator userValidator, IPasswordValidator passwordValidator, IBus bus, ISettings settings, IDatabaseConnectionUserModifier databaseConnectionModifier, IArcUserXmlWriter arcUserXmlWriter)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
            _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _databaseConnectionModifier = databaseConnectionModifier ?? throw new ArgumentNullException(nameof(databaseConnectionModifier));
            _arcUserXmlWriter = arcUserXmlWriter ?? throw new ArgumentNullException(nameof(arcUserXmlWriter));
        }

        public async Task AssociateToUserGroup(string userId, string userGroupName, string requestUserId)
        {
            var user = await _authorizationService.AssociateUserToUserGroup(userId, userGroupName);
            await PublishUserCreatedOrUpdated(user, null, requestUserId);
        }

        public async Task<OutputUserDto> Create(InputUserDto inputUserDto, string requestUserId, string suggestedId = null)
        {
            _userValidator.VerifyExistences(inputUserDto?.SubstituteUsers, inputUserDto?.UserGroups);

            var user = _mapper.Map<InputUserDto, User>(inputUserDto);

            user.Id = suggestedId;
            inputUserDto.Active = inputUserDto.Active ?? true;

            var authenticationType = GetAuthenticationType(inputUserDto.AuthenticationType);
            if (authenticationType == AuthenticationType.AzureAD && string.IsNullOrWhiteSpace(inputUserDto.Email))
                throw new InvalidOperationException($"Email must be provided for authentication type {AuthenticationType.AzureAD}");
            var password = (authenticationType == AuthenticationType.ActiveDirectory) ? null : (inputUserDto.Password ?? string.Empty);

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var createdUser = await _userRepository.Create(user, password);

                bool successfulResult = false;
                if (createdUser != null)
                {
                    var creationResult = CreateUpdateUserARC(inputUserDto, inputUserDto.ArcXml, createdUser);
                    successfulResult = creationResult.Success && creationResult.Changed;
                }

                _userRepository.DefineTransaction(successfulResult);
            }
            catch (Exception)
            {
                _userRepository.DefineTransaction(false);
                throw;
            }

            return _mapper.Map<User, OutputUserDto>(user);
        }

        public async Task<OutputUserDto> Create(ArcUserDto arcUserDto, string requestUserId, string suggestedId = null)
        {
            Validate(arcUserDto);

            _userValidator.VerifyExistences(arcUserDto.SubstituteUsers, arcUserDto.UserGroups);

            var user = _mapper.Map<User>(arcUserDto);
            user.Id = suggestedId;

            var password = (arcUserDto.AuthenticationType == AuthenticationType.ActiveDirectory || arcUserDto.AuthenticationType == AuthenticationType.AzureAD) ? null : (arcUserDto.Password ?? string.Empty);

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var createdUser = await _userRepository.Create(user, password);

                if (createdUser == null)
                    throw new AppException(Constants.Exception.cst_NullUser);

                var arcUser = _mapper.Map<ArcUser>(arcUserDto);

                var arcXml = _arcUserXmlWriter.Write(arcUser, GetDatabaseAuthenticationStringValueAttribute(arcUserDto.AuthenticationType.Value), createdUser.PasswordHistory);
                var createUpdateResult = CreateUpdateUserARC(arcUserDto, arcXml.ToString(), createdUser);

                if (!createUpdateResult.Success)
                    throw new InvalidOperationException($"{Constants.Exception.cst_UserCreationFailed} Error: '{createUpdateResult.Error}'");

                if (!createUpdateResult.Changed)
                    throw new InvalidOperationException(Constants.Exception.cst_UserCreationFailed);    

                _userRepository.DefineTransaction(true);

                return _mapper.Map<OutputUserDto>(createdUser);
            }
            catch (Exception)
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public async Task Delete(string userId, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var userDeleted = await _userRepository.Remove(userId);

                bool successfulResult = false;
                if (userDeleted)
                {
                    _userRepository.ConfigureParametersToRemove(out var parameters, userId);
                    successfulResult = _userRepository.RemoveItemDirectory(Constants.cst_SpRemoveItemDiretorio, parameters);
                }

                _userRepository.DefineTransaction(successfulResult);
            }
            catch (Exception)
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public async Task DissociateFromUserGroup(string userId, string userGroupName, string requestUserId)
        {
            var user = await _authorizationService.DissociateUserFromUserGroup(userId, userGroupName);
            await PublishUserCreatedOrUpdated(user, null, requestUserId);
        }

        public async Task<IReadOnlyCollection<OutputUserDto>> Get(string login)
        {
            if (login == null)
                return await GetAll();

            return await GetByLogin(login);
        }

        public async Task<OutputUserDto> GetById(string userId)
        {
            var user = await _userRepository.GetById(userId);
            var userDto = _mapper.Map<User, OutputUserDto>(user);

            return userDto;
        }

        public OutputUserDto GetById(string userId, out string password)
        {
            var user = _userRepository.GetById(userId).Result;
            var userDto = _mapper.Map<User, OutputUserDto>(user);

            password = userDto.AuthenticationType == AuthenticationType.DatabaseUser ? user.PasswordHash : string.Empty;
            return userDto;
        }

        public async Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId)
        {
            var user = await _userRepository.GetById(userId);
            var userGroups = user.UserGroupUsers.Select(ugu => ugu.UserGroup).ToArray();
            var userGroupDtos = _mapper.Map<UserGroup[], OutputUserGroupDto[]>(userGroups);

            return userGroupDtos;
        }

        public async Task<OutputUserDto> Update(string userId, InputUserDto inputUserDto, string requestUserId)
        {
            var user = await _userRepository.GetById(userId);

            var updatedUser = ValidationUpdateUser(inputUserDto, user);

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var savedUser = await _userRepository.Update(updatedUser, inputUserDto.Password);

                var successfulResult = false;
                if (savedUser != null)
                {
                     var updateResult = CreateUpdateUserARC(inputUserDto, inputUserDto.ArcXml, savedUser);
                    successfulResult = updateResult.Success && updateResult.Changed;
                }

                _userRepository.DefineTransaction(successfulResult);

                return _mapper.Map<User, OutputUserDto>(user);
            }
            catch (Exception)
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public async Task<OutputUserDto> Update(string userId, ArcUserDto arcUserDto, string requestUserId)
        {
            Validate(arcUserDto);

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var currentUser = await _userRepository.GetById(userId);
                AdjustCollections(arcUserDto, currentUser);

                var updatedUser = _mapper.Map(arcUserDto, currentUser);

                var savedUser = await _userRepository.Update(updatedUser, arcUserDto.Password);

                if (savedUser == null)
                    throw new AppException(Constants.Exception.cst_NullUser);

                var arcUser = _mapper.Map<ArcUser>(arcUserDto);

                var arcXml = _arcUserXmlWriter.Write(arcUser, GetDatabaseAuthenticationStringValueAttribute(arcUserDto.AuthenticationType.Value), savedUser.PasswordHistory);
                var createUpdateResult = CreateUpdateUserARC(arcUserDto, arcXml.ToString(), savedUser);

                if (!createUpdateResult.Success)
                    throw new InvalidOperationException($"{Constants.Exception.cst_UserUpdateFailed} Error: '{createUpdateResult.Error}'");

                if (!createUpdateResult.Changed)
                    throw new InvalidOperationException(Constants.Exception.cst_UserUpdateFailedNoChange);

                _userRepository.DefineTransaction(true);

                return _mapper.Map<OutputUserDto>(savedUser);
            }
            catch (Exception)
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public async Task<OutputUserDto> CreateApi(InputUserDto inputUserDto, string requestUserId, string suggestedId = null)
        {
            _userValidator.VerifyExistences(inputUserDto.SubstituteUsers, inputUserDto.UserGroups);
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);

            var user = _mapper.Map<InputUserDto, User>(inputUserDto);

            user.Id = suggestedId;

            var authenticationType = GetAuthenticationType(inputUserDto.AuthenticationType);
            var password = (authenticationType == AuthenticationType.ActiveDirectory) ? null : (inputUserDto.Password ?? string.Empty);

            var createdUser = await _userRepository.Create(user, password);
            return await PublishUserCreatedOrUpdated(createdUser, inputUserDto.Password, requestUserId);
        }

        public async Task DeleteApi(string userId, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            await _userRepository.Remove(userId);

            await _bus.Publish(
                new UserDeletedEvent
                {
                    UserId = userId,
                    RequestUserId = requestUserId
                });
        }

        public async Task<OutputUserDto> UpdateApi(string userId, InputUserDto inputUserDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            var user = await _userRepository.GetById(userId);

            var updatedUser = ValidationUpdateUser(inputUserDto, user);

            var savedUser = await _userRepository.Update(updatedUser, inputUserDto.Password);
            return await PublishUserCreatedOrUpdated(savedUser, inputUserDto.Password, requestUserId);
        }

        private CreateUpdateResult CreateUpdateUserARC(UserBaseDto newUserDto, string userArcXml, User createdUser)
        {
            _userRepository.ConfigureParametersToCreateUpdate(out var parameters, newUserDto.Login, newUserDto.PasswordExpiration.Value.DateTime, createdUser.Id, userArcXml, Constants.cst_Usuario, Constants.cst_Usr);
            _ = _userRepository.CreateUpdateItemDirectory(Constants.cst_SpAtualizaItemDiretorio, parameters);
            var createUpdateResult = CreateUpdateResult.FromParameters(parameters);
            return createUpdateResult;
        }

        private async Task<IReadOnlyCollection<OutputUserDto>> GetByLogin(string login)
        {
            var user = await _userRepository.GetByName(login);
            var userDto = _mapper.Map<User, OutputUserDto>(user);

            return new[] { userDto };
        }

        private async Task<IReadOnlyCollection<OutputUserDto>> GetAll()
        {
            var users = await _userRepository.GetAll();
            var userDtos = _mapper.Map<IReadOnlyCollection<User>, IReadOnlyCollection<OutputUserDto>>(users);

            return userDtos;
        }

        private static void AdjustCollections(UserBaseDto inputUserDto, User user)
        {
            if (inputUserDto.UserGroups == null)
                inputUserDto.UserGroups = user.UserGroupUsers.Select(ugu => ugu.UserGroupId).ToArray();

            if (inputUserDto.SubstituteUsers == null)
                inputUserDto.SubstituteUsers = user.UserSubstitutions.Select(us => us.SubstituteUserId).ToArray();
        }

        private async Task<OutputUserDto> PublishUserCreatedOrUpdated(User user, string hashArc, string requestUserId)
        {
            var userDto = _mapper.Map<User, OutputUserDto>(user);
            var authenticationType = GetAuthenticationType(userDto.AuthenticationType);
            if (userDto.AuthenticationType == null)
                userDto.AuthenticationType = authenticationType;

            await _bus.Publish(
                new UserCreatedOrUpdatedEvent
                {
                    User = userDto,
                    AuthenticationType = authenticationType,
                    PasswordHash = user.PasswordHash,
                    HashArc = hashArc,
                    RequestUserId = requestUserId
                });

            return userDto;
        }

        private AuthenticationType GetAuthenticationType(AuthenticationType? userAuthenticationType)
        {
            var settingsAuthenticationType = _settings.AuthenticationType;

            if (settingsAuthenticationType == SettingsAuthenticationType.User)
                return userAuthenticationType ?? AuthenticationType.ActiveDirectory;

            return (AuthenticationType)settingsAuthenticationType;
        }

        private User ValidationUpdateUser(InputUserDto inputUserDto, User user)
        {
            var autenticationType = GetAuthenticationType(inputUserDto.AuthenticationType);
            if (inputUserDto.Password != null && autenticationType != AuthenticationType.ActiveDirectory)
                _passwordValidator.Validate(inputUserDto.Password);

            AdjustCollections(inputUserDto, user);

            var updatedUser = _mapper.Map(inputUserDto, user);

            if (!string.IsNullOrWhiteSpace(inputUserDto.Login) && inputUserDto.Login != updatedUser.UserName) // If the inputLogin is not null, then the userName should be updated.
            {
                _userValidator.Validate(inputUserDto.Login);
                updatedUser.UserName = inputUserDto.Login;
            }
            return updatedUser;
        }

        private void Validate(ArcUserDto user)
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

            if (!string.IsNullOrWhiteSpace(user.Password))
                _passwordValidator.Validate(user.Password);
        }

        private static string GetDatabaseAuthenticationStringValueAttribute(AuthenticationType authenticationType)
        {
            FieldInfo fieldInfo = authenticationType.GetType().GetField(authenticationType.ToString());
            CompatibilityStringValueAttribute attribute = (CompatibilityStringValueAttribute)fieldInfo.GetCustomAttribute(typeof(CompatibilityStringValueAttribute), false);

            if (attribute != null)
            {
                return attribute.StringValue;
            }
            else
            {
                throw new InvalidOperationException("CompatibilityStringValueAttribute not found.");
            }
        }

        private class CreateUpdateResult
        {
            public bool Changed { get; private set; }
            public bool Success { get; private set; }
            public string Error { get; private set; }

            public static CreateUpdateResult FromParameters(List<SqlParameter> parameters)
            {
                if (parameters is null)
                    throw new ArgumentNullException(nameof(parameters));

                string pError = parameters.FirstOrDefault(p => p.ParameterName == Constants.cst_Erro)?.Value as string;
                return new CreateUpdateResult()
                {
                    Changed = parameters.FirstOrDefault(p => p.ParameterName == Constants.cst_FoiAlterado)?.Value as bool? ?? false,
                    Success = string.IsNullOrWhiteSpace(pError),
                    Error = pError
                };
            }
        }
    }
}
