using MassTransit;
using Microsoft.Data.SqlClient;
using Moq;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Infraestrutura.ServicosCliente;
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
using Identidade.Dominio.Servicos;

namespace Identidade.UnitTests
{
    public class UserGroupClientServiceTests
    {
        [Fact]
        public async Task AddPermission_PublishesEvent_AndReturnsDto()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            authorizationServiceMoq
                .Setup(s => s.AddPermissionsIntoUserGroup(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, int>>()))
                .ReturnsAsync(userGroup);

            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group1", Name = "New Group1" };
            fabricaGrupoUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>()))
                .Returns(outputUserGroupDto);

            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var permissions = new InputPermissionDto { Id = "permission", Operations = new[] { PermissionOperation.All.ToString() } };
            var permissionList = new List<InputPermissionDto>() { permissions };

            permissionOperationManagerMoq
                .Setup(p => p.GetSomaOperacoes(It.IsAny<IEnumerable<string>>()))
                .Returns(1);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.AddPermissions(userGroup.Id, permissionList, "");

            Assert.Equal(outputUserGroupDto.Id, retorno.Id);
            busMoq.Verify(v => v.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePermissions_PublishesEvent_AndReturnsDto()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            authorizationServiceMoq
                .Setup(s => s.DeletePermissionsFromUserGroup(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>()))
                .ReturnsAsync(userGroup);

            var outputUserGroupDto = new OutputUserGroupDto { Id = "Group1", Name = "New Group1" };
            fabricaGrupoUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>()))
                .Returns(outputUserGroupDto);

            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var permissionList = new List<string>() { "01", "02", "03" };

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.DeletePermissions(userGroup.Id, permissionList, "");

            Assert.Equal(outputUserGroupDto.Id, retorno.Id);
            busMoq.Verify(v => v.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_CallsRepositoryAndDirectoryUpdate()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };

            fabricaGrupoUsuarioMoq
                .Setup(s => s.MapearParaGrupoUsuarioAsync(It.IsAny<InputUserGroupDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userGroup);

            fabricaGrupoUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>()))
                .Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = userGroup.Name, ArcXml = inputUserGroupDto.ArcXml });

            userGroupRepositoryMoq.Setup(s => s.Create(It.IsAny<UserGroup>())).ReturnsAsync(userGroup);
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));

            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.Create(inputUserGroupDto, "", "Group");

            Assert.Equal(userGroup.Id, retorno.Id);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsDirectoryRemove()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            userGroupRepositoryMoq.Setup(s => s.RemoveByName(It.IsAny<string>())).ReturnsAsync("id");
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);
            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupDeletedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            await svc.Delete("", "");

            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
        }

        [Fact]
        public async Task Get_UserGroupNameNull_GetAll()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            IReadOnlyCollection<UserGroup> userGroupReadOnlyCollection = new List<UserGroup>(new UserGroup[] { userGroup });
            userGroupRepositoryMoq.Setup(s => s.GetAll()).ReturnsAsync(userGroupReadOnlyCollection);

            fabricaGrupoUsuarioMoq
                .Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>()))
                .Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = userGroup.Name });

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.Get(null);

            Assert.Equal(userGroup.Id, retorno.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetAll(), Times.Once);
        }

        [Fact]
        public async Task Get_UserGroupNameNotNull_GetByName()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            userGroupRepositoryMoq.Setup(s => s.GetByName(It.IsAny<string>())).ReturnsAsync(userGroup);
            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>())).Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = userGroup.Name });

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.Get("New Group");

            Assert.Equal(userGroup.Id, retorno.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetByName(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetById()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(userGroup);
            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>())).Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = userGroup.Name });

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.GetById("Group");

            Assert.Equal(userGroup.Id, retorno.Id);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetPermissions_MapsPermissionsViaFabricaPermissao()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var permission = new Permission { Id = "permission", Name = "permissionName" };
            var userGroupPermission = new UserGroupPermission() { PermissionId = "permissionId", UserGroup = userGroup, Permission = permission };

            var userGroup1 = new UserGroup { Id = "Group", Name = "New Group", UserGroupPermissions = new List<UserGroupPermission> { userGroupPermission } };
            userGroupRepositoryMoq.Setup(s => s.GetByName(It.IsAny<string>())).ReturnsAsync(userGroup1);

            var outputPermissionDtoList = new[] { new OutputPermissionDto { Id = "outputPermissionDto", Name = "outputPermissionDtoName" } };
            fabricaPermissaoMoq.Setup(s => s.MapearParaDtoSaidaPermissao(It.IsAny<Permission[]>())).Returns(outputPermissionDtoList);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.GetPermissions("Group");

            Assert.Equal("outputPermissionDto", retorno.Select(s => s.Id).FirstOrDefault());
            userGroupRepositoryMoq.Verify(v => v.GetByName(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Update_UsesFabricaGrupoUsuarioToBuildPermissions_AndDirectoryUpdate()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var userGroupUpdated = new UserGroup { Id = "Group", Name = "New Group" };
            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml", Permissions = new[] { new InputPermissionDto { Id = "p1" } } };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(userGroup);
            fabricaGrupoUsuarioMoq
                .Setup(s => s.ConstruirPermissoesGrupoUsuarioAsync(It.IsAny<UserGroup>(), It.IsAny<IReadOnlyCollection<InputPermissionDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserGroupPermission>());

            userGroupRepositoryMoq.Setup(s => s.Update(It.IsAny<UserGroup>())).ReturnsAsync(userGroupUpdated);
            userGroupRepositoryMoq.Setup(s => s.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            userGroupRepositoryMoq.Setup(s => s.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(true);

            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>())).Returns(new OutputUserGroupDto { Id = userGroupUpdated.Id, Name = "New Group Updated" });
            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.Update("Group", inputUserGroupDto, "");

            Assert.Equal("New Group Updated", retorno.Name);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.Update(It.IsAny<UserGroup>()), Times.Once);
        }

        [Fact]
        public async Task CreateApi_DoesNotCallDirectoryUpdate()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };

            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaGrupoUsuarioAsync(It.IsAny<InputUserGroupDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(userGroup);
            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>())).Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = userGroup.Name });
            userGroupRepositoryMoq.Setup(s => s.Create(It.IsAny<UserGroup>())).ReturnsAsync(userGroup);
            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.CreateApi(inputUserGroupDto, "", "Group");

            Assert.Equal(userGroup.Id, retorno.Id);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateApi_DoesNotCallDirectoryUpdate()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            var userGroup = new UserGroup { Id = "Group", Name = "New Group" };
            var inputUserGroupDto = new InputUserGroupDto { Name = "New Group", ArcXml = "xml" };

            userGroupRepositoryMoq.Setup(s => s.GetById(It.IsAny<string>())).ReturnsAsync(userGroup);
            userGroupRepositoryMoq.Setup(s => s.Update(It.IsAny<UserGroup>())).ReturnsAsync(userGroup);
            fabricaGrupoUsuarioMoq.Setup(s => s.MapearParaDtoSaidaGrupoUsuario(It.IsAny<UserGroup>())).Returns(new OutputUserGroupDto { Id = userGroup.Id, Name = "New Group Updated" });
            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupCreatedOrUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            var retorno = await svc.UpdateApi("Group", inputUserGroupDto, "");

            Assert.Equal("New Group Updated", retorno.Name);
            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToCreateUpdate(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>(), It.IsAny<DateTime>(),
                 It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.CreateUpdateItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.GetById(It.IsAny<string>()), Times.Once);
            userGroupRepositoryMoq.Verify(v => v.Update(It.IsAny<UserGroup>()), Times.Once);
        }

        [Fact]
        public async Task DeleteApiAsync_DoesNotCallDirectoryRemove()
        {
            var userGroupRepositoryMoq = new Mock<IRepository<UserGroup>>();
            var authorizationServiceMoq = new Mock<IAuthorizationService>();
            var busMoq = new Mock<IBus>();
            var permissionOperationManagerMoq = new Mock<IPermissaoOperacaoHelper>();
            var fabricaGrupoUsuarioMoq = new Mock<IFabricaGrupoUsuario>();
            var fabricaPermissaoMoq = new Mock<IFabricaPermissao>();

            userGroupRepositoryMoq.Setup(s => s.Remove(It.IsAny<string>())).ReturnsAsync(true);
            busMoq.Setup(s => s.Publish(It.IsAny<UserGroupDeletedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var svc = new UserGroupClientService(
                userGroupRepositoryMoq.Object,
                authorizationServiceMoq.Object,
                busMoq.Object,
                permissionOperationManagerMoq.Object,
                Mock.Of<IDatabaseConnectionUserModifier>(),
                fabricaGrupoUsuarioMoq.Object,
                fabricaPermissaoMoq.Object);

            await svc.DeleteApi("", "");

            userGroupRepositoryMoq.Verify(v => v.ConfigureParametersToRemove(out It.Ref<List<SqlParameter>>.IsAny, It.IsAny<string>()), Times.Never);
            userGroupRepositoryMoq.Verify(v => v.RemoveItemDirectory(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Never);
        }
    }
}
