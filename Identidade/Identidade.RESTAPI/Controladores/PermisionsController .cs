using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Identidade.Dominio.Helpers;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Serilog;
using Identidade.RESTAPI.Controladores;
using Identidade.Infraestrutura.ServicosCliente;

namespace Identidade.RESTAPI.Controllers
{
    [Route("permissions")]
    public class PermissionsController : BaseController
    {
        private readonly IPermissionClientService _permissionService;

        public PermissionsController(IPermissionClientService permissionService, TelemetryClient telemetryClient, ILogger logger)
            : base(telemetryClient, logger)
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
            return await ExecuteAsync(async () =>
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
            }, "GetPermissionById", new Dictionary<string, string> { { "PermissionId", permissionId } });
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
            return await ExecuteAsync(async () =>
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
            }, "GetPermissions", new Dictionary<string, string> { { "PermissionNameFilter", permissionName } });
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
            return await ExecuteAsync(async () =>
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
            }, "GetPermissionUserGroups", new Dictionary<string, string> { { "PermissionId", permissionId } });
        }
    }
}
