using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Identidade.Dominio.Helpers;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Identidade.RESTAPI.Controllers
{
    [Authorize]
    [Route("permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionClientService _permissionService;

        public PermissionsController(IPermissionClientService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Gets a permission by the ID.
        /// </summary>
        /// <param name="permissionId"> The ID of the permission to be requested. </param>
        [HttpGet("{permissionId}")]
        [ProducesResponseType(typeof(OutputPermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(string permissionId)
        {
            try
            {
                var permissionDto = await _permissionService.GetById(permissionId);
                return Ok(permissionDto);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Gets all the permissions from the database, allowing filter by name.
        /// </summary>
        /// <param name="permissionName"> The name of the permission to be requested.
        /// Remark: This parameter must be in a URL-encoded string format. </param>
        /// <remarks> Remarks:
        /// <br />
        /// <para> If the name is informed on the query, an array with a single permission is returned. </para></remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<OutputPermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery(Name = "name")] string permissionName)
        {
            try
            {
                var permissionDtos = await _permissionService.Get(permissionName);
                return Ok(permissionDtos);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }

        /// <summary>
        /// Gets all the user groups containing a specific permission.
        /// </summary>
        /// <param name="permissionId"> The ID of the permission contained in the user groups. </param>
        [HttpGet("{permissionId}/groups")]
        [ProducesResponseType(typeof(IReadOnlyCollection<InputUserGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserGroups(string permissionId)
        {
            try
            {
                var userGroupDtos = await _permissionService.GetUserGroups(permissionId);
                return Ok(userGroupDtos);
            }
            catch (NotFoundAppException e)
            {
                return NotFound(e.Errors);
            }
        }
    }
}
