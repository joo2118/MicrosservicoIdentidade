using AutoMapper;
using MassTransit;
using Microsoft.Data.SqlClient;
using Moq;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Dominio.Servicos;
using Identidade.Infraestrutura.ClientServices;
using Identidade.Infraestrutura.Helpers;
using Identidade.Publico.Dtos;
using Identidade.Publico.Enumerations;
using Identidade.Publico.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Identidade.UnitTests
{
    public class UserGroupClientServiceTests
    {
        [Fact]
        public void AddPermission()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            authorizationServiceMoq.Setup(s => s.AddPermissionsIntoUserGroup(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, int>>())).Returns(Task.FromResult(userGroup));

            var outputUserGroupDto = new OutputUserGroupDto
            {
                Id = "Group1",
                Name = "New Group1",
            };
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            var userCreatedOrUpdatedEvent = new UserGroupCreatedOrUpdatedEvent
            {
                UserGroup = outputUserGroupDto,
                RequestUserId = ""
            };

            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(userCreatedOrUpdatedEvent));

            var permissions = new InputPermissionDto { Id = "permission", Operations = new[] { PermissionOperation.All.ToString() } };
            var permissionList = new List<InputPermissionDto>() { permissions };

            var userGroupClientServiceMoq = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());


            var retorno = userGroupClientServiceMoq.AddPermissions(userGroup.Id, permissionList, "");

            Assert.Equal(retorno.Result.Id, outputUserGroupDto.Id);
            busMoq.Verify(v => v.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void DeletePermissions()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            authorizationServiceMoq.Setup(s => s.DeletePermissionsFromUserGroup(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(userGroup));

            var outputUserGroupDto = new OutputUserGroupDto
            {
                Id = "Group1",
                Name = "New Group1",
            };
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            var userCreatedOrUpdatedEvent = new UserGroupCreatedOrUpdatedEvent
            {
                UserGroup = outputUserGroupDto,
                RequestUserId = ""
            };

            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(userCreatedOrUpdatedEvent));

            var permissionList = new List<string>() { "01", "02", "03" };

            var userGroupClientServiceMoq = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());


            var retorno = userGroupClientServiceMoq.DeletePermissions(userGroup.Id, permissionList, "");

            Assert.Equal(retorno.Result.Id, outputUserGroupDto.Id);
            busMoq.Verify(v => v.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Create()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup {Name = "New Group",  };

            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group", ArcXml = "xml" };

            mapperMoq.Setup(s => s.Map<InputUserGroupDto, UserGroup>(It.IsAny<InputUserGroupDto>())).Returns(userGroup);
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);
            userGroupRepositoryMoq.Setup(s => s.Create(It.IsAny<UserGroup>())).Returns(Task.FromResult(userGroup));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.Create(inputUserGroupDto, "", "Group");

            Assert.Equal(retorno.Result.Id, userGroup.Id);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny , It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);

            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            userGroupRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).Returns(Task.FromResult(true));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            await userGroupClientService.Delete("", "");

            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
        }

        [Fact]
        public void Get_UserGroupNameNull_GetAll()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            IReadOnlyCollection<UserGroup> userGroupReadOnlyCollection = new List<UserGroup>(new UserGroup[] { userGroup });            
            userGroupRepositoryMoq.Setup(s => s.GetAll()).Returns(Task.FromResult(userGroupReadOnlyCollection));

            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group" };
            IReadOnlyCollection<OutputUserGroupDto> outputUserReadOnlyCollection = new List<OutputUserGroupDto>(new OutputUserGroupDto[] { outputUserGroupDto });

            mapperMoq.Setup(s => s.Map<IReadOnlyCollection<UserGroup>, IReadOnlyCollection<OutputUserGroupDto>>(userGroupReadOnlyCollection)).Returns(outputUserReadOnlyCollection);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.Get(null);

            Assert.Equal(userGroup.Id, retorno.Result.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetAll(), Times.Once);
        }

        [Fact]
        public void Get_UserGroupNameNotNull_GetByName()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group" };

            userGroupRepositoryMoq.Setup(s => s.GetByName(It.IsAny<string>())).Returns(Task.FromResult(userGroup));
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.Get("New Group");

            Assert.Equal(userGroup.Id, retorno.Result.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetByName(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetById()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group" };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(userGroup));
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.GetById("Group");

            Assert.Equal(userGroup.Id, retorno.Result.Id);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetPermissions()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var permission = new Permission { Id = "permission", Name = "permissionName" };
            var userGroupPermission = new UserGroupPermission() { PermissionId = "permissionId", UserGroup = userGroup, Permission = permission };
            var userGroupPermissionList = new List<UserGroupPermission>() { userGroupPermission };

            var userGroup1 = new UserGroup { Id = "Group", Name = "New Group", UserGroupPermissions = userGroupPermissionList };
            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(userGroup1));

            var outputPermissionDto = new OutputPermissionDto() { Id = "outputPermissionDto", Name = "outputPermissionDtoName", Operations = new[] { PermissionOperation.All.ToString() }};
            var outputPermissionDtoList = new OutputPermissionDto[] { outputPermissionDto };

            mapperMoq.Setup(s => s.Map<Permission[], OutputPermissionDto[]>(It.IsAny<Permission[]>())).Returns(outputPermissionDtoList);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.GetPermissions("Group");

            Assert.Equal("outputPermissionDto", retorno.Result.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Update()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var userGroupUpdated = new UserGroup { Id = "Group", Name = "New Group" };
            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group Updated" };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(userGroup));
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserGroupDto>(), It.IsAny<UserGroup>())).Returns(userGroup);
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            userGroupRepositoryMoq.Setup(s => s.Update(It.IsAny<UserGroup>())).Returns(Task.FromResult(userGroupUpdated));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.Update("Group", inputUserGroupDto, "");

            Assert.Equal(retorno.Result.Name, outputUserGroupDto.Name);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.Update(It.IsAny<UserGroup>()), Times.Once);
        }

        [Fact]
        public void CreateApi()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Name = "New Group", };

            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group", ArcXml = "xml" };

            mapperMoq.Setup(s => s.Map<InputUserGroupDto, UserGroup>(It.IsAny<InputUserGroupDto>())).Returns(userGroup);
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);
            userGroupRepositoryMoq.Setup(s => s.Create(It.IsAny<UserGroup>())).Returns(Task.FromResult(userGroup));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.CreateApi(inputUserGroupDto, "", "Group");

            Assert.Equal(retorno.Result.Id, userGroup.Id);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);

            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
        }

        [Fact]
        public void UpdateApi()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var userGroupUpdated = new UserGroup { Id = "Group", Name = "New Group" };
            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };
            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group", Name = "New Group Updated" };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).Returns(Task.FromResult(userGroup));
            mapperMoq.Setup(s => s.Map(It.IsAny<InputUserGroupDto>(), It.IsAny<UserGroup>())).Returns(userGroup);
            mapperMoq.Setup(s => s.Map<UserGroup, OutputUserGroupDto>(It.IsAny<UserGroup>())).Returns(outputUserGroupDto);

            userGroupRepositoryMoq.Setup(s => s.Update(It.IsAny<UserGroup>())).Returns(Task.FromResult(userGroupUpdated));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            var retorno = userGroupClientService.UpdateApi("Group", inputUserGroupDto, "");

            Assert.Equal(retorno.Result.Name, outputUserGroupDto.Name);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.Update(It.IsAny<UserGroup>()), Times.Once);
        }

        [Fact]
        public async Task DeleteApiAsync()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var mapperMoq = new Mock<IMapper>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();

            userGroupRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).Returns(Task.FromResult(true));
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            var userGroupClientService = new UserGroupClientService(userGroupRepositoryMoq.Object, authorizationServiceMoq.Object, mapperMoq.Object, busMoq.Object, permissionOperationManagerMoq.Object, Mock.Of<IDatabaseConnectionUserModifier>());
            await userGroupClientService.DeleteApi("", "");

            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
        }
    }
}
