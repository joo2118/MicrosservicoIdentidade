using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
using Identidade.Dominio.Repositorios;
using Identidade.Infraestrutura.Resilience;
using Polly;

namespace Identidade.Infraestrutura.ServicosCliente
{
    public interface IUserClientService
    {
        Task AssociateToUserGroup(string userId, string userGroupName, string requestUserId);
        Task<OutputUserDto> Create(InputUserDto inputUserDto, string requestUserId, string suggestedId = null);
        Task<OutputUserDto> Create(ArcUserDto arcUserDto, string requestUserId, string suggestedId = null);
        Task<OutputUserDto> CreateApi(InputUserDto inputUserDto, string requestUserId, string suggestedId = null);
        Task Delete(string userId, string requestUserId);
        Task DissociateFromUserGroup(string userId, string userGroupName, string requestUserId);
        Task<IReadOnlyCollection<OutputUserDto>> Get(string login);
        Task<IReadOnlyCollection<OutputUserDto>> Get(string login, int? page, int? pageSize);
        Task<OutputUserDto> GetById(string userId);
        OutputUserDto GetById(string userId, out string password);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId, int? page, int? pageSize);
        Task<OutputUserDto> Update(string userId, InputUserDto inputUserDto, string requestUserId);
        Task<OutputUserDto> Update(string userId, ArcUserDto arcUserDto, string requestUserId);
        Task<OutputUserDto> UpdateApi(string userId, InputUserDto inputUserDto, string requestUserId);
        Task<ResultadoPaginado<OutputUserDto>> GetPaginado(string login, int? page, int? pageSize);
        Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginado(string userId, int? page, int? pageSize);
    }

    public class UserClientService : IUserClientService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserValidator _userValidator;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IBus _bus;
        private readonly ISettings _settings;
        private readonly IDatabaseConnectionUserModifier _databaseConnectionModifier;
        private readonly IArcUserXmlWriter _arcUserXmlWriter;
        private readonly IFabricaUsuario _fabricaUsuario;

        private readonly ResiliencePipeline _pipeline;
        private readonly ResiliencePipeline _busPublishPipeline;

        public UserClientService(IUserRepository userRepository, IAuthorizationService authorizationService,
            IUserValidator userValidator, IPasswordValidator passwordValidator, IBus bus, ISettings settings, IDatabaseConnectionUserModifier databaseConnectionModifier, IArcUserXmlWriter arcUserXmlWriter,
            IFabricaUsuario fabricaUsuario)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
            _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _databaseConnectionModifier = databaseConnectionModifier ?? throw new ArgumentNullException(nameof(databaseConnectionModifier));
            _arcUserXmlWriter = arcUserXmlWriter ?? throw new ArgumentNullException(nameof(arcUserXmlWriter));
            _fabricaUsuario = fabricaUsuario ?? throw new ArgumentNullException(nameof(fabricaUsuario));

            _pipeline = FabricaPipelineResiliencia.Create();
            _busPublishPipeline = FabricaPipelineResiliencia.CreateBusPublish();
        }

        public Task AssociateToUserGroup(string userId, string userGroupName, string requestUserId) =>
            ExecuteResilientAsync(() => AssociateToUserGroupCore(userId, userGroupName, requestUserId));

        private async Task AssociateToUserGroupCore(string userId, string userGroupName, string requestUserId)
        {
            var user = await _authorizationService.AssociateUserToUserGroup(userId, userGroupName);
            await PublishUserCreatedOrUpdated(user, null, requestUserId);
        }

        public Task<OutputUserDto> Create(InputUserDto inputUserDto, string requestUserId, string suggestedId = null) =>
            ExecuteResilientAsync(() => CreateCore(inputUserDto, requestUserId, suggestedId));

        private async Task<OutputUserDto> CreateCore(InputUserDto inputUserDto, string requestUserId, string suggestedId = null)
        {
            _userValidator.VerifyExistences(inputUserDto?.SubstituteUsers, inputUserDto?.UserGroups);

            var user = await _fabricaUsuario.MapearParaUsuarioAsync(inputUserDto);

            user.Id = suggestedId;
            inputUserDto.Active = inputUserDto.Active ?? true;

            var authenticationType = GetAuthenticationType(inputUserDto.AuthenticationType);
            if (authenticationType == AuthenticationType.AzureAD && string.IsNullOrWhiteSpace(inputUserDto.Email))
                throw new InvalidOperationException($"Email must be provided for authentication type {AuthenticationType.AzureAD}");
            var password = authenticationType == AuthenticationType.ActiveDirectory ? null : inputUserDto.Password ?? string.Empty;

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
            catch
            {
                _userRepository.DefineTransaction(false);
                throw;
            }

            return _fabricaUsuario.MapearParaDtoSaidaUsuario(user);
        }

        public Task<OutputUserDto> Create(ArcUserDto arcUserDto, string requestUserId, string suggestedId = null) =>
            ExecuteResilientAsync(() => CreateCore(arcUserDto, requestUserId, suggestedId));

        private async Task<OutputUserDto> CreateCore(ArcUserDto arcUserDto, string requestUserId, string suggestedId = null)
        {
            _fabricaUsuario.ValidarArcUserDto(arcUserDto);

            _userValidator.VerifyExistences(arcUserDto.SubstituteUsers, arcUserDto.UserGroups);

            var user = await _fabricaUsuario.MapearParaUsuarioAsync(arcUserDto);
            user.Id = suggestedId;

            var password = arcUserDto.AuthenticationType == AuthenticationType.ActiveDirectory || arcUserDto.AuthenticationType == AuthenticationType.AzureAD ? null : arcUserDto.Password ?? string.Empty;

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var createdUser = await _userRepository.Create(user, password);

                if (createdUser == null)
                    throw new AppException(Constants.Exception.cst_NullUser);

                var arcUser = _fabricaUsuario.MapearParaArcUser(arcUserDto);

                var arcXml = _arcUserXmlWriter.Write(arcUser, GetDatabaseAuthenticationStringValueAttribute(arcUserDto.AuthenticationType!.Value), createdUser.PasswordHistory);
                var createUpdateResult = CreateUpdateUserARC(arcUserDto, arcXml.ToString(), createdUser);

                if (!createUpdateResult.Success)
                    throw new InvalidOperationException($"{Constants.Exception.cst_UserCreationFailed} Error: '{createUpdateResult.Error}'");

                if (!createUpdateResult.Changed)
                    throw new InvalidOperationException(Constants.Exception.cst_UserCreationFailed);

                _userRepository.DefineTransaction(true);
                await PublishUserCreatedOrUpdated(createdUser, null, requestUserId);

                return _fabricaUsuario.MapearParaDtoSaidaUsuario(createdUser);
            }
            catch
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public Task Delete(string userId, string requestUserId) =>
            ExecuteResilientAsync(() => DeleteCore(userId, requestUserId));

        private async Task DeleteCore(string userId, string requestUserId)
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

                if (successfulResult)
                    await ExecuteResilientAsync(() => _bus.Publish(
                        new UserDeletedEvent
                        {
                            UserId = userId,
                            RequestUserId = requestUserId
                        }));
            }
            catch
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public Task DissociateFromUserGroup(string userId, string userGroupName, string requestUserId) =>
            ExecuteResilientAsync(() => DissociateFromUserGroupCore(userId, userGroupName, requestUserId));

        private async Task DissociateFromUserGroupCore(string userId, string userGroupName, string requestUserId)
        {
            var user = await _authorizationService.DissociateUserFromUserGroup(userId, userGroupName);
            await PublishUserCreatedOrUpdated(user, null, requestUserId);
        }

        public Task<IReadOnlyCollection<OutputUserDto>> Get(string login) =>
            Get(login, page: null, pageSize: null);

        public Task<IReadOnlyCollection<OutputUserDto>> Get(string login, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetCore(login, page, pageSize));

        private async Task<IReadOnlyCollection<OutputUserDto>> GetCore(string login, int? page, int? pageSize)
        {
            if (login == null)
                return await GetAll(page, pageSize);

            return await GetByLogin(login);
        }

        public Task<OutputUserDto> GetById(string userId) =>
            ExecuteResilientAsync(() => GetByIdCore(userId));

        private async Task<OutputUserDto> GetByIdCore(string userId)
        {
            var user = await _userRepository.GetById(userId);
            return _fabricaUsuario.MapearParaDtoSaidaUsuario(user);
        }

        public OutputUserDto GetById(string userId, out string password)
        {
            // Avoid sync-over-async deadlocks by blocking with GetAwaiter().GetResult().
            var user = ExecuteResilientAsync(() => _userRepository.GetById(userId)).GetAwaiter().GetResult();
            var userDto = _fabricaUsuario.MapearParaDtoSaidaUsuario(user);

            var authenticationType = GetAuthenticationType(userDto.AuthenticationType);
            if (userDto.AuthenticationType == null)
                userDto.AuthenticationType = authenticationType;

            password = authenticationType == AuthenticationType.DatabaseUser ? user.PasswordHash : string.Empty;
            return userDto;
        }

        public Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId) =>
            GetUserGroups(userId, page: null, pageSize: null);

        public Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string userId, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetUserGroupsCore(userId, page, pageSize));

        private async Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroupsCore(string userId, int? page, int? pageSize)
        {
            var user = await _userRepository.GetById(userId);
            var userGroups = user.UserGroupUsers.Select(ugu => ugu.UserGroup).AsQueryable();

            if (page.HasValue || pageSize.HasValue)
            {
                var pagination = new OpcoesPaginacao(page, pageSize);
                userGroups = userGroups.Skip(pagination.Skip).Take(pagination.TamanhoPagina);
            }

            return userGroups
                .Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt })
                .ToArray();
        }

        public Task<OutputUserDto> Update(string userId, InputUserDto inputUserDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateCore(userId, inputUserDto, requestUserId));

        private async Task<OutputUserDto> UpdateCore(string userId, InputUserDto inputUserDto, string requestUserId)
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

                await PublishUserCreatedOrUpdated(user, inputUserDto.Password, requestUserId);

                return _fabricaUsuario.MapearParaDtoSaidaUsuario(user);
            }
            catch
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public Task<OutputUserDto> Update(string userId, ArcUserDto arcUserDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateCore(userId, arcUserDto, requestUserId));

        private async Task<OutputUserDto> UpdateCore(string userId, ArcUserDto arcUserDto, string requestUserId)
        {
            _fabricaUsuario.ValidarArcUserDto(arcUserDto);

            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            _userRepository.BeginTransaction();
            try
            {
                var currentUser = await _userRepository.GetById(userId);
                AdjustCollections(arcUserDto, currentUser);

                var updatedUser = await _fabricaUsuario.MapearParaUsuarioAsync(arcUserDto, originalUserLogin: currentUser.UserName);
                updatedUser.Id = currentUser.Id;

                var savedUser = await _userRepository.Update(updatedUser, arcUserDto.Password);

                if (savedUser == null)
                    throw new AppException(Constants.Exception.cst_NullUser);

                var arcUser = _fabricaUsuario.MapearParaArcUser(arcUserDto);

                var arcXml = _arcUserXmlWriter.Write(arcUser, GetDatabaseAuthenticationStringValueAttribute(arcUserDto.AuthenticationType!.Value), savedUser.PasswordHistory);
                var createUpdateResult = CreateUpdateUserARC(arcUserDto, arcXml.ToString(), savedUser);

                if (!createUpdateResult.Success)
                    throw new InvalidOperationException($"{Constants.Exception.cst_UserUpdateFailed} Error: '{createUpdateResult.Error}'");

                if (!createUpdateResult.Changed)
                    throw new InvalidOperationException(Constants.Exception.cst_UserUpdateFailedNoChange);

                _userRepository.DefineTransaction(true);

                return _fabricaUsuario.MapearParaDtoSaidaUsuario(savedUser);
            }
            catch
            {
                _userRepository.DefineTransaction(false);
                throw;
            }
        }

        public Task<OutputUserDto> CreateApi(InputUserDto inputUserDto, string requestUserId, string suggestedId = null) =>
            ExecuteResilientAsync(() => CreateApiCore(inputUserDto, requestUserId, suggestedId));

        private async Task<OutputUserDto> CreateApiCore(InputUserDto inputUserDto, string requestUserId, string suggestedId = null)
        {
            _userValidator.VerifyExistences(inputUserDto.SubstituteUsers, inputUserDto.UserGroups);
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);

            var user = await _fabricaUsuario.MapearParaUsuarioAsync(inputUserDto);

            user.Id = suggestedId;

            var authenticationType = GetAuthenticationType(inputUserDto.AuthenticationType);
            var password = authenticationType == AuthenticationType.ActiveDirectory ? null : inputUserDto.Password ?? string.Empty;

            var createdUser = await _userRepository.Create(user, password);
            return await PublishUserCreatedOrUpdated(createdUser, inputUserDto.Password, requestUserId);
        }

        public Task<OutputUserDto> UpdateApi(string userId, InputUserDto inputUserDto, string requestUserId) =>
            ExecuteResilientAsync(() => UpdateApiCore(userId, inputUserDto, requestUserId));

        private async Task<OutputUserDto> UpdateApiCore(string userId, InputUserDto inputUserDto, string requestUserId)
        {
            _databaseConnectionModifier.ModifyConnection(_userRepository.GetContext(), requestUserId);
            var user = await _userRepository.GetById(userId);

            var updatedUser = ValidationUpdateUser(inputUserDto, user);

            var savedUser = await _userRepository.Update(updatedUser, inputUserDto.Password);
            return await PublishUserCreatedOrUpdated(savedUser, inputUserDto.Password, requestUserId);
        }

        public Task<ResultadoPaginado<OutputUserDto>> GetPaginado(string login, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetPaginadoCore(login, page, pageSize));

        private async Task<ResultadoPaginado<OutputUserDto>> GetPaginadoCore(string login, int? page, int? pageSize)
        {
            if (login == null)
                return await GetAllPaginado(page, pageSize);

            var items = await GetByLogin(login);
            return new ResultadoPaginado<OutputUserDto>
            {
                Items = items,
                Pagina = 1,
                TamanhoPagina = items.Count,
                Total = items.Count
            };
        }

        public Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginado(string userId, int? page, int? pageSize) =>
            ExecuteResilientAsync(() => GetUserGroupsPaginadoCore(userId, page, pageSize));

        private async Task<ResultadoPaginado<OutputUserGroupDto>> GetUserGroupsPaginadoCore(string userId, int? page, int? pageSize)
        {
            var user = await _userRepository.GetById(userId);
            var userGroups = user.UserGroupUsers.Select(ugu => ugu.UserGroup).AsQueryable();

            var pagination = new OpcoesPaginacao(page, pageSize);
            var result = await userGroups
                .Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt })
                .ParaResultadoPaginado(pagination);

            return result;
        }

        private async Task<ResultadoPaginado<OutputUserDto>> GetAllPaginado(int? page, int? pageSize)
        {
            var pagination = new OpcoesPaginacao(page, pageSize);
            var usersQuery = (await _userRepository.GetAll()).AsQueryable();

            var result = await usersQuery
                .Select(u => _fabricaUsuario.MapearParaDtoSaidaUsuario(u))
                .ParaResultadoPaginado(pagination);

            return result;
        }

        private CreateUpdateResult CreateUpdateUserARC(UserBaseDto newUserDto, string userArcXml, User createdUser)
        {
            _userRepository.ConfigureParametersToCreateUpdate(out var parameters, newUserDto.Login, newUserDto.PasswordExpiration.Value.DateTime, createdUser.Id, userArcXml, Constants.cst_Usuario, Constants.cst_Usr);
            _ = _userRepository.CreateUpdateItemDirectory(Constants.cst_SpAtualizaItemDiretorio, parameters);
            return CreateUpdateResult.FromParameters(parameters);
        }

        private async Task<IReadOnlyCollection<OutputUserDto>> GetByLogin(string login)
        {
            var user = await _userRepository.GetByName(login);
            var userDto = _fabricaUsuario.MapearParaDtoSaidaUsuario(user);

            return new[] { userDto };
        }

        private async Task<IReadOnlyCollection<OutputUserDto>> GetAll(int? page, int? pageSize)
        {
            var users = await _userRepository.GetAll(page, pageSize);
            var dtos = users.Select(u => _fabricaUsuario.MapearParaDtoSaidaUsuario(u)).ToArray();
            return dtos;
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
            var userDto = _fabricaUsuario.MapearParaDtoSaidaUsuario(user);
            var authenticationType = GetAuthenticationType(userDto.AuthenticationType);
            if (userDto.AuthenticationType == null)
                userDto.AuthenticationType = authenticationType;

            await ExecuteResilientBusPublishAsync(() => _bus.Publish(
                new UserCreatedOrUpdatedEvent
                {
                    User = userDto,
                    AuthenticationType = authenticationType,
                    PasswordHash = user.PasswordHash,
                    HashArc = hashArc,
                    RequestUserId = requestUserId
                }));

            return userDto;
        }

        private Task ExecuteResilientBusPublishAsync(Func<Task> action) =>
            _busPublishPipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();

        private Task ExecuteResilientAsync(Func<Task> action) =>
            _pipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();

        private Task<T> ExecuteResilientAsync<T>(Func<Task<T>> action) =>
            _pipeline.ExecuteAsync(async _ =>
            {
                try
                {
                    return await action().ConfigureAwait(false);
                }
                catch (SqlException ex) when (DetectorErroSQLTransitorio.ErroTransient(ex))
                {
                    throw new ExceptionTransitoriaSQL("Transient SQL error.", ex);
                }
            }).AsTask();

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

            var updatedUser = _fabricaUsuario
                .MapearParaUsuarioAsync(inputUserDto, originalUserLogin: user.UserName)
                .GetAwaiter()
                .GetResult();

            if (!string.IsNullOrWhiteSpace(inputUserDto.Login) && inputUserDto.Login != updatedUser.UserName)
            {
                _userValidator.Validate(inputUserDto.Login);
                updatedUser.UserName = inputUserDto.Login;
            }

            return updatedUser;
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
