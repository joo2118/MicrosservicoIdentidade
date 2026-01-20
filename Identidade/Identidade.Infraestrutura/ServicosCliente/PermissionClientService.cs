using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identidade.Infraestrutura.ClientServices
{
    public interface IPermissionClientService
    {
        Task<OutputPermissionDto> GetById(string permissionId);
        Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName);
        Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId);
    }

    public class PermissionClientService : IPermissionClientService
    {
        private readonly IReadOnlyRepository<Permission> _permissionRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFabricaPermissao _fabricaPermissao;

        public PermissionClientService(IReadOnlyRepository<Permission> permissionRepository, IAuthorizationService authorizationService, IFabricaPermissao fabricaPermissao)
        {
            _permissionRepository = permissionRepository;
            _authorizationService = authorizationService;
            _fabricaPermissao = fabricaPermissao;
        }

        public async Task<OutputPermissionDto> GetById(string permissionId)
        {
            var permission = await _permissionRepository.GetById(permissionId);
            return _fabricaPermissao.MapearParaDtoSaidaPermissao(permission);
        }

        public async Task<IReadOnlyCollection<OutputPermissionDto>> Get(string permissionName)
        {
            if (permissionName == null)
                return await GetAll();

            return await GetByName(permissionName);
        }

        public async Task<IReadOnlyCollection<OutputUserGroupDto>> GetUserGroups(string permissionId)
        {
            var userGroups = await _authorizationService.GetUserGroupsContainigPermission(permissionId);
            return userGroups.Select(ug => new OutputUserGroupDto { Id = ug.Id, Name = ug.Name, CreatedAt = ug.CreatedAt, LastUpdatedAt = ug.LastUpdatedAt }).ToArray();
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetAll()
        {
            var permissions = await _permissionRepository.GetAll();
            return permissions.Select(_fabricaPermissao.MapearParaDtoSaidaPermissao).ToArray();
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetByName(string permissionName)
        {
            var permission = await _permissionRepository.GetByName(permissionName);
            return new[] { _fabricaPermissao.MapearParaDtoSaidaPermissao(permission) };
        }
    }
}
