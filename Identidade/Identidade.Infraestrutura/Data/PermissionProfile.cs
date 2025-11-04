using AutoMapper;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;

namespace Identidade.Infraestrutura.Data
{
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            CreateMap<Permission, OutputPermissionDto>()
                .ForMember(permissionDto => permissionDto.Operations,
                    ex => ex.MapFrom(permission => new[] { PermissionOperation.All.ToString() }));
        }
    }
}
