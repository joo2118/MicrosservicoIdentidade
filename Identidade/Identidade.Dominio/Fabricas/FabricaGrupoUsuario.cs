using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;

namespace Identidade.Dominio.Fabricas
{
    public sealed class FabricaGrupoUsuario : IFabricaGrupoUsuario
    {
        private readonly IReadOnlyRepository<Permission> _permissionRepository;
        private readonly IPermissaoOperacaoHelper _permissionOperationManager;

        public FabricaGrupoUsuario(
            IReadOnlyRepository<Permission> permissionRepository,
            IPermissaoOperacaoHelper permissionOperationManager)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _permissionOperationManager = permissionOperationManager ?? throw new ArgumentNullException(nameof(permissionOperationManager));
        }

        public Task<UserGroup> MapearParaGrupoUsuarioAsync(InputUserGroupDto dto, CancellationToken cancellationToken = default)
            => MapToUserGroupAsync(dto, cancellationToken);

        public OutputUserGroupDto MapearParaDtoSaidaGrupoUsuario(UserGroup userGroup)
            => MapToOutputUserGroupDto(userGroup);

        public Task<ICollection<UserGroupPermission>> ConstruirPermissoesGrupoUsuarioAsync(
            UserGroup userGroup,
            IReadOnlyCollection<InputPermissionDto> permissionDtos,
            CancellationToken cancellationToken = default)
            => BuildUserGroupPermissionsAsync(userGroup, permissionDtos, cancellationToken);

        public async Task<UserGroup> MapToUserGroupAsync(InputUserGroupDto dto, CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("User group name can not be empty.");

            var userGroup = new UserGroup
            {
                Name = dto.Name
            };

            userGroup.UserGroupPermissions = await BuildUserGroupPermissionsAsync(
                userGroup,
                dto.Permissions,
                cancellationToken);

            return userGroup;
        }

        public OutputUserGroupDto MapToOutputUserGroupDto(UserGroup userGroup)
        {
            if (userGroup is null)
                return null;

            return new OutputUserGroupDto
            {
                Id = userGroup.Id,
                Name = userGroup.Name,
                CreatedAt = userGroup.CreatedAt,
                LastUpdatedAt = userGroup.LastUpdatedAt,
                Permissions = userGroup.UserGroupPermissions?.Select(p => new InputPermissionDto
                {
                    Id = p.PermissionId,
                    Operations = _permissionOperationManager.GetOperacoes(p.PermissionOperations)
                }).ToArray() ?? Array.Empty<InputPermissionDto>()
            };
        }

        public async Task<ICollection<UserGroupPermission>> BuildUserGroupPermissionsAsync(
            UserGroup userGroup,
            IReadOnlyCollection<InputPermissionDto> permissionDtos,
            CancellationToken cancellationToken = default)
        {
            if (permissionDtos is null || permissionDtos.Count == 0)
                return Array.Empty<UserGroupPermission>();

            var missingIdsFromInput = permissionDtos
                .Where(p => p == null || string.IsNullOrWhiteSpace(p.Id))
                .Select(_ => "<empty>")
                .ToArray();

            if (missingIdsFromInput.Length > 0)
                throw new AppException("Permission id can not be null or empty.");

            var byId = permissionDtos
                .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);

            var ids = byId.Keys.ToArray();

            var permissionsById = await _permissionRepository.GetByIds(ids);

            var missing = ids
                .Where(id => !permissionsById.ContainsKey(id))
                .ToArray();

            if (missing.Length > 0)
                throw new NotFoundAppException("Permissions", "ID", string.Join(", ", missing));

            var result = new List<UserGroupPermission>(permissionDtos.Count);
            foreach (var permissionDto in permissionDtos)
            {
                var permission = permissionsById[permissionDto.Id];
                var operationSum = _permissionOperationManager.GetSomaOperacoes(permissionDto.Operations ?? Array.Empty<string>());
                result.Add(new UserGroupPermission(userGroup, permission, operationSum));
            }

            return result;
        }
    }
}
