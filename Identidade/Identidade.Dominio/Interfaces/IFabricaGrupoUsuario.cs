using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;

namespace Identidade.Dominio.Interfaces
{
    public interface IFabricaGrupoUsuario
    {
        /// <summary>
        /// Mapeia <see cref="InputUserGroupDto"/> para um <see cref="UserGroup"/> e executa validações de domínio,
        /// incluindo a verificação da existência das permissões referenciadas.
        /// </summary>
        Task<UserGroup> MapearParaGrupoUsuarioAsync(
            InputUserGroupDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mapeia <see cref="UserGroup"/> para <see cref="OutputUserGroupDto"/>.
        /// </summary>
        OutputUserGroupDto MapearParaDtoSaidaGrupoUsuario(UserGroup userGroup);

        /// <summary>
        /// Constrói os relacionamentos <see cref="UserGroupPermission"/> a partir dos DTOs de permissão,
        /// garantindo que as permissões referenciadas existam.
        /// </summary>
        Task<ICollection<UserGroupPermission>> ConstruirPermissoesGrupoUsuarioAsync(
            UserGroup userGroup,
            IReadOnlyCollection<InputPermissionDto> permissionDtos,
            CancellationToken cancellationToken = default);
    }
}
