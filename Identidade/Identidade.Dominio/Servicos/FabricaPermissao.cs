using System.Collections.Generic;
using System.Linq;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;

namespace Identidade.Dominio.Servicos
{
    public sealed class FabricaPermissao : IFabricaPermissao
    {
        public OutputPermissionDto MapearParaDtoSaidaPermissao(Permission permission)
        {
            if (permission is null)
                return null;

            return new OutputPermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Operations = new[] { PermissionOperation.All.ToString() }
            };
        }

        public IReadOnlyCollection<OutputPermissionDto> MapearParaDtoSaidaPermissao(IEnumerable<Permission> permissions)
        {
            if (permissions is null)
                return [];

            return permissions
                .Select(MapearParaDtoSaidaPermissao)
                .Where(dto => dto is not null)
                .ToArray();
        }
    }
}