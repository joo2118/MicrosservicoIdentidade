using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Servicos;
using Identidade.Dominio.Repositorios;

namespace Identidade.UnitTests.Helpers
{
    public class UserGroupRepositoryBuilder
    {
        private IARCDbContext _arcDbContext = Mock.Of<IARCDbContext>();
        private IUpdateConcurrencyResolver _updateConcurrencyResolver = Mock.Of<IUpdateConcurrencyResolver>();
        private IIdGenerator _idGenerator = Mock.Of<IIdGenerator>();
        public UserGroupRepositoryBuilder WithARCDbContext(IARCDbContext arcDbContext)
        {
            _arcDbContext = arcDbContext;
            return this;
        }
        public UserGroupRepositoryBuilder WithUpdateConcurrencyResolver(IUpdateConcurrencyResolver updateConcurrencyResolver)
        {
            _updateConcurrencyResolver = updateConcurrencyResolver;
            return this;
        }

        public UserGroupRepositoryBuilder WithIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
            return this;
        }
        public UserGroupRepository Build() =>
            new UserGroupRepository(_arcDbContext, _updateConcurrencyResolver, _idGenerator);
    }
}
