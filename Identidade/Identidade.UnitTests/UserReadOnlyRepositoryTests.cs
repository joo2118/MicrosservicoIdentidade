using Microsoft.Data.SqlClient;
using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Identidade.Dominio.Modelos;

namespace Identidade.UnitTests
{
    public class UserReadOnlyRepositoryTests
    {
        [Fact]
        public void BeginTransaction()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            userReadOnlyRepository.BeginTransaction();

            arcDbContext.Verify(v => v.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void CommitTransaction()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            userReadOnlyRepository.DefineTransaction(true);

            arcDbContext.Verify(v => v.CommitTransaction(), Times.Once);
            arcDbContext.Verify(v => v.RollbackTransaction(), Times.Never);
        }
        [Fact]
        public void RollbackTransaction()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            userReadOnlyRepository.DefineTransaction(false);

            arcDbContext.Verify(v => v.RollbackTransaction(), Times.Once);
            arcDbContext.Verify(v => v.CommitTransaction(), Times.Never);
        }

        [Fact]
        public void ConfigureParametersToCreateUpdate()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            var nome = "Teste";
            var xml = "xml";
            var prefixo = "prefixo";
            var id = "xpto";

            userReadOnlyRepository.ConfigureParametersToCreateUpdate(out var parameters, nome, DateTime.Now, id, xml, 17, prefixo);

            Assert.Equal(parameters[0].Value, prefixo);
            Assert.Equal(parameters[1].Value, id);
            Assert.Equal(parameters[2].Value, nome);
        }

        [Fact]
        public void ConfigureParametersToRemove()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            var id = "xpto";

            userReadOnlyRepository.ConfigureParametersToRemove(out var parameters, id);

            Assert.Equal(parameters[1].Value, id);
        }
        [Fact]
        public void CreateUpdateItemDirectory()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.CreateUpdateArcUser(sql, parameters))
                .Callback((string sql, List<SqlParameter> parameters) =>
                {
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value = true;
                })
                .Returns(1);

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());
            var retorno = userReadOnlyRepository.CreateUpdateItemDirectory(sql, parameters);

            Assert.True(retorno);
            arcDbContext.Verify(v => v.CreateUpdateArcUser(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
            Assert.True((bool)parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value);
        }

        [Fact]
        public void CreateUpdateItemDirectoryFailOnException()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            var errorMessage = "Error on test";

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.CreateUpdateArcUser(sql, parameters))
                .Throws(new Exception(errorMessage));

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.CreateUpdateItemDirectory(sql, parameters));
            Assert.Equal(parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value, errorMessage);
        }

        [Fact]
        public void CreateUpdateItemDirectoryFailWhenError()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            var errorMessage = "Error on test";

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.CreateUpdateArcUser(sql, parameters))
                .Callback((string sql, List<SqlParameter> parameters) =>
                {
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value = false;
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value = errorMessage;
                });

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.CreateUpdateItemDirectory(sql, parameters));
            Assert.Equal(parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value, errorMessage);
        }

        [Fact]
        public void CreateUpdateItemDirectoryFailWhenNotChanged()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.CreateUpdateArcUser(sql, parameters))
                .Callback((string sql, List<SqlParameter> parameters) =>
                {
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value = false;
                });

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.CreateUpdateItemDirectory(sql, parameters));
        }

        [Fact]
        public void RemoveItemDirectory()
        {
            var arcDbContext = new Mock<IARCDbContext>();
            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            arcDbContext.Setup(s => s.RemoveArcUser(It.IsAny<string>(), It.IsAny<List<SqlParameter>>())).Returns(1);
            userReadOnlyRepository.ConfigureParametersToRemove(out var parameters, "");
            parameters[3].Value = true;
            var retorno = userReadOnlyRepository.RemoveItemDirectory(Constants.cst_SpRemoveItemDiretorio, parameters);

            arcDbContext.Verify(v => v.RemoveArcUser(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()), Times.Once);
            Assert.True(retorno);
        }

        [Fact]
        public void RemoveItemDirectoryFailOnException()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            var errorMessage = "Error on test";

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.RemoveArcUser(sql, parameters))
                .Throws(new Exception(errorMessage));

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.RemoveItemDirectory(sql, parameters));
            Assert.Equal(parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value, errorMessage);
        }

        [Fact]
        public void RemoveItemDirectoryFailWhenError()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };
            var errorMessage = "Error on test";

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.RemoveArcUser(sql, parameters))
                .Callback((string sql, List<SqlParameter> parameters) =>
                {
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value = false;
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value = errorMessage;
                });

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.RemoveItemDirectory(sql, parameters));
            Assert.Equal(parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value, errorMessage);
        }

        [Fact]
        public void RemoveItemDirectoryFailWhenNotChanged()
        {
            string sql = "sql-test";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter(Constants.cst_Erro, null),
                new SqlParameter(Constants.cst_FoiAlterado, false)
            };

            var arcDbContext = new Mock<IARCDbContext>();
            arcDbContext
                .Setup(s => s.RemoveArcUser(sql, parameters))
                .Callback((string sql, List<SqlParameter> parameters) =>
                {
                    parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value = false;
                });

            var userReadOnlyRepository = new UserReadOnlyRepository(arcDbContext.Object, Mock.Of<IUpdateConcurrencyResolver>());

            Assert.False(userReadOnlyRepository.RemoveItemDirectory(sql, parameters));
        }
    }
}
