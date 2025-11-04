using AutoMapper;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Publico.Dtos;
using System.Collections.Generic;
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
        private readonly IMapper _mapper;

        public PermissionClientService(IReadOnlyRepository<Permission> permissionRepository, IAuthorizationService authorizationService, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<OutputPermissionDto> GetById(string permissionId)
        {
            var permission = await _permissionRepository.GetById(permissionId);
            var permissionDto = _mapper.Map<Permission, OutputPermissionDto>(permission);

            return permissionDto;
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
            var userGroupsDto = _mapper.Map<IReadOnlyCollection<UserGroup>, IReadOnlyCollection<OutputUserGroupDto>>(userGroups);

            return userGroupsDto;
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetAll()
        {
            var permissions = await _permissionRepository.GetAll();
            var permissionDtos = _mapper.Map<IReadOnlyCollection<Permission>, IReadOnlyCollection<OutputPermissionDto>>(permissions);

            return permissionDtos;
        }

        private async Task<IReadOnlyCollection<OutputPermissionDto>> GetByName(string permissionName)
        {
            var permission = await _permissionRepository.GetByName(permissionName);
            var permissionDto = _mapper.Map<Permission, OutputPermissionDto>(permission);

            return new[] { permissionDto };
        }
    }
}
