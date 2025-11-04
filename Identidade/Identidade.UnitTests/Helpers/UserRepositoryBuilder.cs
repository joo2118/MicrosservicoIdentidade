using Moq;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Servicos;

namespace Identidade.UnitTests.Helpers
{
    internal class UserRepositoryBuilder
    {
        private IARCDbContext _arcDbContext = Mock.Of<IARCDbContext>();
        private IUserValidator _userValidator = Mock.Of<IUserValidator>();
        private IPasswordValidator _passwordValidator = Mock.Of<IPasswordValidator>();
        private IUpdateConcurrencyResolver _updateConcurrencyResolver = Mock.Of<IUpdateConcurrencyResolver>();
        private IIdGenerator _idGenerator = Mock.Of<IIdGenerator>();

        public UserRepositoryBuilder WithARCDbContext(IARCDbContext arcDbContext)
        {
            _arcDbContext = arcDbContext;
            return this;
        }

        public UserRepositoryBuilder WithUserNameValidator(IUserValidator userValidator)
        {
            _userValidator = userValidator;
            return this;
        }

        public UserRepositoryBuilder WithPasswordValidator(IPasswordValidator passwordValidator)
        {
            _passwordValidator = passwordValidator;
            return this;
        }

        public UserRepositoryBuilder WithUpdateConcurrencyResolver(IUpdateConcurrencyResolver updateConcurrencyResolver)
        {
            _updateConcurrencyResolver = updateConcurrencyResolver;
            return this;
        }

        public UserRepositoryBuilder WithIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
            return this;
        }

        public UserRepository Build() =>
            new UserRepository(_arcDbContext, _userValidator, _passwordValidator,
                _updateConcurrencyResolver, _idGenerator);
    }
}
