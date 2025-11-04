//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Identidade.Dominio.Helpers;
//using Identidade.Dominio.Modelos;
//using Identidade.Dominio.Servicos;
//using System;
//using System.Threading.Tasks;
//using Identidade.Infraestrutura.Data;
//using Identidade.UnitTests.Helpers;
//using Xunit;
//using Xunit.Gherkin.Quick;

//namespace Identidade.UnitTests
//{
    //[FeatureFile("./UpdateUserFeature.feature")]
    //public class UpdateUserFeature : Feature, IDisposable
    //{
    //    private User _updatedUser;
    //    private DateTime _before;
    //    private DateTime _after;
    //    private readonly DbContextOptions<ARCDbContext> _options;
    //    private readonly SqliteConnection _connection;

    //    public UpdateUserFeature()
    //    {
    //        _connection = new SqliteConnection("DataSource=:memory:");
    //        _connection.Open();

    //        _options = new DbContextOptionsBuilder<ARCDbContext>()
    //            .UseSqlite(_connection)
    //            .Options;

    //        using (var context = new ARCDbContext(_options, Mock.Of<IPasswordHasher>()))
    //        {
    //            context.Database.EnsureCreated();
    //        }
    //    }

    //    [Given(@"an existing user")]
    //    [Given(@"an existing user with passwordHash ""EncriptedPassword""")]
    //    public async Task Given_an_existing_user()
    //    {
    //        using (var context = new ARCDbContext(_options, Mock.Of<IPasswordHasher>()))
    //        {
    //            var user = new User
    //            {
    //                Id = "USR_GUID",
    //                UserName = "user",
    //                PasswordHash = "EncriptedPassword",
    //                PasswordHistory = "EncriptedPassword"
    //            };

    //            await context.Users.AddAsync(user);
    //            await context.SaveChangesAsync();
    //        }
    //    }

        //[When(@"I update the user")]
        //[When(@"I update the passwordHash for ""NewEncriptedPassword""")]
        //public async Task When_I_update_the_user()
        //{
        //    using (var context = new ARCDbContext(_options, Mock.Of<IPasswordHasher>()))
        //    {
        //        var userRepository = new UserRepositoryBuilder()
        //            .WithARCDbContext(context)
        //            .WithUpdateConcurrencyResolver(new UpdateConcurrencyResolver(context))
        //            .Build();

        //        var user = new User
        //        {
        //            Id = "USR_GUID",
        //            UserName = "user",
        //            PasswordHash = "NewEncriptedPassword",
        //            PasswordHistory = "EncriptedPassword"
        //        };

        //        _before = DateTime.UtcNow;
        //        _updatedUser = await userRepository.Update(user, null);
        //        _after = DateTime.UtcNow;
        //    }
        //}

        //[Then(@"the LastUpdatedAt proterty is updated")]
        //public async Task Then_the_LastUpdatedAt_proterty_is_updated()
        //{
        //    Assert.Equal("user", _updatedUser.UserName);
        //    Assert.Equal("USR_GUID", _updatedUser.Id);
        //    Assert.True(_before < _updatedUser.LastUpdatedAt && _updatedUser.LastUpdatedAt < _after);

        //    using (var context = new ARCDbContext(_options, Mock.Of<IPasswordHasher>()))
        //    {
        //        var updatedUser = await context.Users.FindAsync("USR_GUID");

        //        Assert.Equal("user", updatedUser.UserName);
        //        Assert.Equal("USR_GUID", updatedUser.Id);
        //        Assert.True(_before < updatedUser.LastUpdatedAt && updatedUser.LastUpdatedAt < _after);
        //    }
        //}
        
        //[Then(@"the password and the passwordHistory are updated")]
        //public async Task Then_the_password_and_the_passwordHistory_are_updated()
        //{
        //    Assert.Equal("user", _updatedUser.UserName);
        //    Assert.Equal("USR_GUID", _updatedUser.Id);
        //    Assert.Equal("NewEncriptedPassword", _updatedUser.PasswordHash);
        //    Assert.Equal("EncriptedPassword;NewEncriptedPassword", _updatedUser.PasswordHistory);

        //    using (var context = new ARCDbContext(_options, Mock.Of<IPasswordHasher>()))
        //    {
        //        var updatedUser = await context.Users.FindAsync("USR_GUID");

        //        Assert.Equal("user", updatedUser.UserName);
        //        Assert.Equal("USR_GUID", updatedUser.Id);
        //        Assert.Equal("NewEncriptedPassword", updatedUser.PasswordHash);
        //        Assert.Equal("EncriptedPassword;NewEncriptedPassword", updatedUser.PasswordHistory);
        //    }
        //}
        
//        public void Dispose()
//        {
//            _connection.Close();
//        }
//    }
//}
