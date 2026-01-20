using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;
using System.Collections.Generic;

namespace Identidade.Dominio.Interfaces
{
    public interface IFabricaPermissao
    {
        OutputPermissionDto MapearParaDtoSaidaPermissao(Permission permission);

        IReadOnlyCollection<OutputPermissionDto> MapearParaDtoSaidaPermissao(IEnumerable<Permission> permissions);
    }
}