using Microsoft.AspNetCore.Mvc;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Identidade.Dominio.Helpers;
using Microsoft.AspNetCore.Authorization;
using Identidade.Dominio.Interfaces;
using System;
using Microsoft.ApplicationInsights;
using Serilog;
using Identidade.RESTAPI.Controladores;
using Identidade.Infraestrutura.ServicosCliente;
using Identidade.RESTAPI.Helpers;

namespace Identidade.RESTAPI.Controllers
{
    [Route("groups")]
    public class UserGroupsController : BaseController
    {
        private readonly IUserGroupClientService _userGroupService;
        private readonly ICredentialsFactory _credentialsFactory;

        public UserGroupsController(IUserGroupClientService userGroupService, ICredentialsFactory credentialsFactory, TelemetryClient telemetryClient, ILogger logger)
            : base(telemetryClient, logger)
        {
            _userGroupService = userGroupService ?? throw new ArgumentNullException(nameof(userGroupService));
            _credentialsFactory = credentialsFactory ?? throw new ArgumentNullException(nameof(credentialsFactory));
        }

        /// <summary>
        /// Creates a new user group on the database.
        /// </summary>
        /// <param name="userGroupDto"> The user group to be created. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpPost]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] InputUserGroupDto userGroupDto, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var createdUserGroupDto = await _userGroupService.Create(userGroupDto, credentials.UserLogin);

                    return Created(credentials.UserLogin, createdUserGroupDto);
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
            }, "CreateUserGroup", new Dictionary<string, string> { { "UserGroupName", userGroupDto?.Name } });
        }

        /// <summary>
        /// Deletes an user group from the database.
        /// </summary>
        /// <param name="userGroupName"> The Name of the user group to be deleted. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userGroupName}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(string userGroupName, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    await _userGroupService.Delete(userGroupName, credentials.UserLogin);
                    return NoContent();
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "DeleteUserGroup", new Dictionary<string, string> { { "UserGroupName", userGroupName } });   
        }

        /// <summary>
        /// Updates an user group.
        /// </summary>
        /// <param name="userGroupName"> The Name of the user group to be updated. </param>
        /// <param name="userGroupDto"> The user group to be updated. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> The properties not setted or setted as null will be ignored on the update. </para>
        /// <br />
        /// <para> If the permissions field is not null (including an empty array), the existing permissions will be overrided. To add permissions without impacting the existing ones, use "PUT groups/{userGroupId}/permissions". </para></remarks>
        [HttpPut("{userGroupName}")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(string userGroupName, [FromBody] InputUserGroupDto userGroupDto, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var updatedUserGroup = await _userGroupService.UpdateByName(userGroupName, userGroupDto, credentials.UserLogin);
                    return Ok(updatedUserGroup);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
                catch (AppException e)
                {
                    return BadRequest(e.Errors);
                }
            }, "UpdateUserGroup", new Dictionary<string, string> { { "UserGroupName", userGroupName } });
        }

        /// <summary>
        /// Gets an user group by the ID.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group to be requested. </param>
        [HttpGet("{userGroupId}")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(string userGroupId)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var userGroupDto = await _userGroupService.GetById(userGroupId);
                    return Ok(userGroupDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUserGroupById", new Dictionary<string, string> { { "UserGroupId", userGroupId } });
        }

        /// <summary>
        /// Gets all the user groups from the database, allowing filter by name.
        /// </summary>
        /// <param name="userGroupName"> The name of the user group to be requested.
        /// Remark: This parameter must be in a URL-encoded string format. </param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> If the name is informed on the query, an array with a single permission is returned. </para></remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputUserGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery] string userGroupName, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var userGroupDto = await _userGroupService.Get(userGroupName, page, pageSize);
                    var projected = ProjectionHelper.ApplyProjection(userGroupDto, projection);
                    return Ok(projected);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUserGroups", new Dictionary<string, string>
            {
                { "UserGroupNameFilter", userGroupName },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets user groups with a paginated result envelope (includes total count).
        /// This endpoint is added for performance comparison; existing GET endpoints remain unchanged.
        /// </summary>
        [HttpGet("paginado")]
        [ProducesResponseType(typeof(ResultadoPaginado<OutputUserGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPaginado([FromQuery] string userGroupName, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _userGroupService.GetPaginado(userGroupName, page, pageSize);
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
            }, "GetUserGroupsPaginado", new Dictionary<string, string>
            {
                { "UserGroupNameFilter", userGroupName },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets all the permissions of an user group.
        /// </summary>
        /// <param name="userGroupName"> The Name of the user group where the permissions will be requested. </param>
        [HttpGet("{userGroupName}/permissions")]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputPermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPermissions(string userGroupName, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var permissionsDto = await _userGroupService.GetPermissions(userGroupName, page, pageSize);
                    var projected = ProjectionHelper.ApplyProjection(permissionsDto, projection);
                    return Ok(projected);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "GetUserGroupPermissions", new Dictionary<string, string>
            {
                { "UserGroupName", userGroupName },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Gets all the permissions of an user group with a paginated result envelope (includes total count).
        /// This endpoint is added for performance comparison; existing GET endpoints remain unchanged.
        /// </summary>
        [HttpGet("{userGroupName}/permissions/paginado")]
        [ProducesResponseType(typeof(ResultadoPaginado<OutputPermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPermissionsPaginado(string userGroupName, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, [FromQuery] string projection = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _userGroupService.GetPermissionsPaginado(userGroupName, page, pageSize);
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
            }, "GetUserGroupPermissionsPaginado", new Dictionary<string, string>
            {
                { "UserGroupName", userGroupName },
                { "Page", page?.ToString() },
                { "PageSize", pageSize?.ToString() },
                { "Projection", projection }
            });
        }

        /// <summary>
        /// Adds new permissions into an user group.
        /// </summary>
        /// <param name="userGroupName"> The Name of the user group where the permissions will be added. </param>
        /// <param name="permissions"> The permissions to be added into the user group. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpPut("{userGroupName}/permissions")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddPermissions(string userGroupName, [FromBody] IReadOnlyCollection<InputPermissionDto> permissions, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var userGroupDto = await _userGroupService.AddPermissions(userGroupName, permissions, credentials.UserLogin);
                    return Ok(userGroupDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "AddPermissionsToUserGroup", new Dictionary<string, string> { { "UserGroupName", userGroupName } });
        }

        /// <summary>
        /// Deletes permissions from an user group.
        /// </summary>
        /// <param name="userGroupName"> The Name of the user group where the permissions will be deleted from. </param>
        /// <param name="permissionsIds"> The IDs of the permissions to be deleted from the user group. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userGroupName}/permissions")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeletePermissions(string userGroupName, [FromBody] IReadOnlyCollection<string> permissionsIds, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            return await ExecuteAsync(async () =>
            {
                try
                {
                    var credentials = _credentialsFactory.Create(authorization, requestUser);
                    var userGroupDto = await _userGroupService.DeletePermissions(userGroupName, permissionsIds, credentials.UserLogin);
                    return Ok(userGroupDto);
                }
                catch (NotFoundAppException e)
                {
                    return NotFound(e.Errors);
                }
            }, "DeletePermissionsFromUserGroup", new Dictionary<string, string> { { "UserGroupName", userGroupName } });
        }
    }
}
