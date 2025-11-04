using Microsoft.AspNetCore.Mvc;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Identidade.Dominio.Helpers;
using Microsoft.AspNetCore.Authorization;
using Identidade.Dominio.Interfaces;
using System;

namespace Identidade.RESTAPI.Controllers
{
    [Authorize]
    [Route("groups")]
    public class UserGroupsController : ControllerBase
    {
        private readonly IUserGroupClientService _userGroupService;
        private readonly ICredentialsFactory _credentialsFactory;

        public UserGroupsController(IUserGroupClientService userGroupService, ICredentialsFactory credentialsFactory)
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
        }

        /// <summary>
        /// Deletes an user group from the database.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group to be deleted. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userGroupId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(string userGroupId, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            try
            {
                var credentials = _credentialsFactory.Create(authorization, requestUser);
                await _userGroupService.Delete(userGroupId, credentials.UserLogin);
                return NoContent();
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Updates an user group.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group to be updated. </param>
        /// <param name="userGroupDto"> The user group to be updated. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> The properties not setted or setted as null will be ignored on the update. </para>
        /// <br />
        /// <para> If the permissions field is not null (including an empty array), the existing permissions will be overrided. To add permissions without impacting the existing ones, use "PUT groups/{userGroupId}/permissions". </para></remarks>
        [HttpPut("{userGroupId}")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(string userGroupId, [FromBody] InputUserGroupDto userGroupDto, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            try
            {
                var credentials = _credentialsFactory.Create(authorization, requestUser);
                var updatedUserGroup = await _userGroupService.Update(userGroupId, userGroupDto, credentials.UserLogin);
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
            try
            {
                var userGroupDto = await _userGroupService.GetById(userGroupId);
                return Ok(userGroupDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
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
        public async Task<IActionResult> Get([FromQuery] string userGroupName)
        {
            try
            {
                var userGroupDto = await _userGroupService.Get(userGroupName);
                return Ok(userGroupDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Gets all the permissions of an user group.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group where the permissions will be requested. </param>
        [HttpGet("{userGroupId}/permissions")]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputPermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetPermissions(string userGroupId)
        {
            try
            {
                var permissionsDto = await _userGroupService.GetPermissions(userGroupId);
                return Ok(permissionsDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Adds new permissions into an user group.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group where the permissions will be added. </param>
        /// <param name="permissions"> The permissions to be added into the user group. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpPut("{userGroupId}/permissions")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddPermissions(string userGroupId, [FromBody] IReadOnlyCollection<InputPermissionDto> permissions, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            try
            {
                var credentials = _credentialsFactory.Create(authorization, requestUser);
                var userGroupDto = await _userGroupService.AddPermissions(userGroupId, permissions, credentials.UserLogin);
                return Ok(userGroupDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Deletes permissions from an user group.
        /// </summary>
        /// <param name="userGroupId"> The ID of the user group where the permissions will be deleted from. </param>
        /// <param name="permissionsIds"> The IDs of the permissions to be deleted from the user group. </param>
        /// <param name="authorization">The authorization token from the header.</param>
        /// <param name="requestUser">The request user from the header (optional).</param>
        [HttpDelete("{userGroupId}/permissions")]
        [ProducesResponseType(typeof(OutputUserGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeletePermissions(string userGroupId, [FromBody] IReadOnlyCollection<string> permissionsIds, [FromHeader] string authorization, [FromHeader] string requestUser = null)
        {
            try
            {
                var credentials = _credentialsFactory.Create(authorization, requestUser);
                var userGroupDto = await _userGroupService.DeletePermissions(userGroupId, permissionsIds, credentials.UserLogin);
                return Ok(userGroupDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }
    }
}
