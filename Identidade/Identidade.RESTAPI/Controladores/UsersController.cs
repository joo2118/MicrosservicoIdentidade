using Microsoft.AspNetCore.Mvc;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Serilog;
using Microsoft.ApplicationInsights;
using Identidade.Infraestrutura.ServicosCliente;
using Identidade.Dominio.Extensoes;
using Identidade.RESTAPI.Helpers;

namespace Identidade.RESTAPI.Controladores
{
    [Route("users")]
    public class UsersController : BaseController
    {
        private readonly IUserClientService _userService;
        private readonly ICredentialsFactory _credentialsFactory;

        public UsersController(IUserClientService userService, ICredentialsFactory credentialsFactory, ILogger logger, TelemetryClient telemetryClient)
            : base(telemetryClient, logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _credentialsFactory = credentialsFactory ?? throw new ArgumentNullException(nameof(credentialsFactory));
        }

        /// <summary>
        /// Creates a new user on the database.
        /// </summary>
        /// <param name="arcUserDto"> A JSON containing the informations of the user to be created. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpPost]
        [ProducesResponseType(typeof(OutputUserDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ArcUserDto arcUserDto, [FromHeader][Required] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    if(arcUserDto is null)
                        return BadRequest("Invalid User provided.");
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var createdUserDto = await _userService.Create(arcUserDto, credentials.UserLogin, arcUserDto.Codigo);
                    
                    return Created(credentials.UserLogin, createdUserDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
                catch (ConflictAppException e)
                {
                    return Conflict(e.Errors);
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, e.GetAllMessages());
                }
            }, "CreateUser", new Dictionary<string, string> { { "UserLogin", arcUserDto?.Login } });
        }

        /// <summary>
        /// Deletes an user from the database.
        /// </summary>
        /// <param name="userId"> The ID of the user to be deleted. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(string userId, [FromHeader][Required] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    await _userService.Delete(userId, credentials.UserLogin);

                    return NoContent();
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }
           , "DeleteUser", new Dictionary<string, string> { { "UserId", userId } });
        }

        /// <summary>
        /// Updates an user.
        /// </summary>
        /// <param name="userId"> The ID of the user to be updated. </param>
        /// <param name="arcUserDto"> A JSON containing the information to be updated. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> The properties not setted or setted as null will be ignored on the update. </para>
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(OutputUserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update([Required] string userId, [FromBody][Required] ArcUserDto arcUserDto, [FromHeader][Required] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    if (arcUserDto is null)
                        return BadRequest("Invalid User provided.");

                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var updatedUserDto = await _userService.Update(userId, arcUserDto, credentials.UserLogin);
                    
                    return Ok(updatedUserDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, e.GetAllMessages());
                }
            }, "UpdateUser", new Dictionary<string, string> { { "UserId", userId } });
        }

        /// <summary>
        /// Patches an user.
        /// </summary>
        /// <param name="userId"> The ID of the user to be updated. </param>
        /// <param name="arcUserDto"> A JSON containing the partial information to be added to the existing user. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="patchUserMerger">The interface implementations responsible for merging users.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> The properties not setted or setted as null will not be modified. </para>
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpPatch("{userId}")]
        [ProducesResponseType(typeof(OutputUserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Patch(string userId, [FromBody] PatchUserDto arcUserDto, [FromHeader] string authorization, [FromServices] IPatchUserMerger patchUserMerger, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    if (arcUserDto is null)
                        return BadRequest("Invalid User provided.");
                    var credentials = _credentialsFactory.Create(authorization, requestUser);

                    var currentUser = _userService.GetById(userId, out var password);
                    if (currentUser is null)
                        throw new NotFoundAppException("user", "ID", userId);

                    var mergedUserDto = patchUserMerger.Merge(currentUser, password, arcUserDto);

                    var updatedUserDto = await _userService.Update(userId, mergedUserDto, credentials.UserLogin);
                    return Ok(updatedUserDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, e.GetAllMessages());
                }
            }, "PatchUser", new Dictionary<string, string> { { "UserId", userId } });
        }

        /// <summary>
        /// Gets an user by the ID.
        /// </summary>
        /// <param name="userId"> The ID of the user to be requested. </param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(OutputUserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(string userId)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var userDto = await _userService.GetById(userId);
                    return Ok(userDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUserById", new Dictionary<string, string> { { "UserId", userId } });
        }

        /// <summary>
        /// Gets all the users from database, allowing filter by login.
        /// </summary>
        /// <param name="login"> The login of the user to be requested. </param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> If the name is informed on the query, an array with a single user is returned. </para>
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputUserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery] string login, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var userDtos = await _userService.Get(login, page, pageSize);
                    var projected = ProjectionHelper.ApplyProjection(userDtos, projection);
                    return Ok(projected);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUsers", new Dictionary<string, string>
            {
                { "LoginFilter", login },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets all the users from database, allowing filter by login.
        /// </summary>
        /// <param name="login"> The login of the user to be requested. </param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> If the name is informed on the query, an array with a single user is returned. </para>
        /// <br />
        /// <para> On the response, the password is never visible. </para></remarks>
        [HttpGet("paginado")]
        [ProducesResponseType(typeof(ResultadoPaginado<OutputUserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPaginado([FromQuery] string login, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _userService.GetPaginado(login, page, pageSize);
                    var projectedItems = ProjectionHelper.ApplyProjection(result.Items, projection);

                    return Ok(new ResultadoPaginado<object>
                    {
                        Items = projectedItems,
                        Pagina = result.Pagina,
                        TamanhoPagina = result.TamanhoPagina,
                        Total = result.Total
                    });
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUsersPaginado", new Dictionary<string, string>
            {
                { "LoginFilter", login },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets all the user groups associated with a specific user.
        /// </summary>
        /// <param name="userId"> The user associated with the user groups. </param>
        [HttpGet("{userId}/groups")]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputUserGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUserGroups(string userId, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var userGroupDtos = await _userService.GetUserGroups(userId, page, pageSize);
                    var projected = ProjectionHelper.ApplyProjection(userGroupDtos, projection);
                    return Ok(projected);
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
            }, "GetUserGroups", new Dictionary<string, string>
            {
                { "UserId", userId },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets user groups for an user with a paginated result envelope (includes total count).
        /// This endpoint is added for performance comparison; existing GET endpoints remain unchanged.
        /// </summary>
        [HttpGet("{userId}/groups/paginado")]
        [ProducesResponseType(typeof(ResultadoPaginado<OutputUserGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUserGroupsPaginado(string userId, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _userService.GetUserGroupsPaginado(userId, page, pageSize);
                    var projectedItems = ProjectionHelper.ApplyProjection(result.Items, projection);

                    return Ok(new ResultadoPaginado<object>
                    {
                        Items = projectedItems,
                        Pagina = result.Pagina,
                        TamanhoPagina = result.TamanhoPagina,
                        Total = result.Total
                    });
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
            }, "GetUserGroupsPaginado", new Dictionary<string, string>
            {
                { "UserId", userId },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Associates an user to a group.
        /// </summary>
        /// <param name="userId"> The user to be associated. </param>
        /// <param name="userGroupName"> The group where the user will be associated to. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpPut("{userId}/groups/{userGroupName}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AssociateToUserGroup(string userId, string userGroupName, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    await _userService.AssociateToUserGroup(userId, userGroupName, credentials.UserLogin);
                    return NoContent();
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
            }, "AssociateUserToGroup", new Dictionary<string, string> { { "UserId", userId }, { "UserGroupName", userGroupName } });
        }

        /// <summary>
        /// Dissociates an user from a group.
        /// </summary>
        /// <param name="userId"> The user to be assigned. </param>
        /// <param name="userGroupName"> The group where the user will be assigned to. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userId}/groups/{userGroupName}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DissociateFromUserGroup(string userId, string userGroupName, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    await _userService.DissociateFromUserGroup(userId, userGroupName, credentials.UserLogin);
                    return NoContent();
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
            }, "DissociateUserFromGroup", new Dictionary<string, string> { { "UserId", userId }, { "UserGroupName", userGroupName } });
        }
    }
}
