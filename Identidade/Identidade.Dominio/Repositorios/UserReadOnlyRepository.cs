using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Identidade.Dominio.Modelos;

namespace Identidade.Dominio.Repositorios
{
    public class UserReadOnlyRepository : IReadOnlyRepository<User>
    {
        protected readonly IARCDbContext _arcDbContext;
        private readonly IUpdateConcurrencyResolver _updateConcurrencyResolver;

        protected bool InTransaction { get; private set; } = false;

        public UserReadOnlyRepository(IARCDbContext arcDbContext, IUpdateConcurrencyResolver updateConcurrencyResolver)
        {
            _arcDbContext = arcDbContext;
            _updateConcurrencyResolver = updateConcurrencyResolver;
        }

        public async Task<User> GetById(string userId)
        {
            var user = await 
                AddUserRelatedData(
                    _arcDbContext.Users.Where(u => u.Id == userId))
                .SingleOrDefaultAsync();

            if (user == null)
                throw new NotFoundAppException("user", "ID", userId);

            return user;
        }

        public async Task<User> GetByName(string userName)
        {
            var user = await
                AddUserRelatedData(
                    _arcDbContext.Users.Where(u => u.UserName == userName))
                .SingleOrDefaultAsync();

            if (user == null)
                throw new NotFoundAppException("user", "user name", userName);

            return user;
        }

        public async Task<IReadOnlyCollection<User>> GetAll() =>
            await AddUserRelatedData(_arcDbContext.Users).ToArrayAsync();

        public void BeginTransaction()
        {
            _arcDbContext.BeginTransaction();
            InTransaction = true;
        }

        public void DefineTransaction(bool successfulResult)
        {
            InTransaction = false;
            if (successfulResult)
            {
                SaveChanges().Wait();
                _arcDbContext.CommitTransaction();
            }
            else
            {
                _arcDbContext.RollbackTransaction();
            }
        }

        protected async Task SaveChanges()
        {
            if (InTransaction)
                return;

            await _updateConcurrencyResolver.SaveChangesSafe();
        }

        public void ConfigureParametersToCreateUpdate(out List<SqlParameter> parameters, string nome, DateTime vencimento, string createdUserId, string content, int classe, string prefixo)
        {
            SqlParameter pID = new SqlParameter(Constants.cst_Id, -1);
            pID.Direction = ParameterDirection.Output;

            SqlParameter pCodigoGerado = new SqlParameter(Constants.cst_CodigoGerado, SqlDbType.VarChar);
            pCodigoGerado.Direction = ParameterDirection.Output;
            pCodigoGerado.Size = 255;

            SqlParameter pFoiAlterado = new SqlParameter(Constants.cst_FoiAlterado, SqlDbType.Bit);
            pFoiAlterado.Direction = ParameterDirection.Output;

            SqlParameter pErro = new SqlParameter(Constants.cst_Erro, SqlDbType.VarChar);
            pErro.Direction = ParameterDirection.Output;
            pErro.Size = 1024;

            parameters = new List<SqlParameter>()
            {
                new SqlParameter(Constants.cst_Prefixo, prefixo),
                new SqlParameter(Constants.cst_Codigo, createdUserId),
                new SqlParameter(Constants.cst_Nome, nome),
                new SqlParameter(Constants.cst_IDClasse, classe),
                new SqlParameter(Constants.cst_Vencimento, vencimento),
                new SqlParameter(Constants.cst_Content, content),
                new SqlParameter(Constants.cst_SomenteLeitura, false),
                pID,
                pCodigoGerado,
                pFoiAlterado,
                pErro
            };
        }
        public void ConfigureParametersToRemove(out List<SqlParameter> parameters, string userId)
        {
            SqlParameter pFoiAlterado = new SqlParameter(Constants.cst_FoiAlterado, SqlDbType.Bit);
            pFoiAlterado.Direction = ParameterDirection.Output;

            parameters = new List<SqlParameter>()
            {
                new SqlParameter(Constants.cst_Id, DBNull.Value),
                new SqlParameter(Constants.cst_Codigo, userId),
                new SqlParameter(Constants.cst_IDClasse, DBNull.Value),
                pFoiAlterado
            };
        }
        public bool CreateUpdateItemDirectory(string sql, List<SqlParameter> parameters)
        {
            try
            {
                _arcDbContext.CreateUpdateArcUser(sql: sql, parameters: parameters);
                var result = parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value;

                return (bool)result;
            }
            catch (Exception ex)
            {
                parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value = ex.Message;
                return false;
            }
        }

        public bool RemoveItemDirectory(string sql, List<SqlParameter> parameters)
        {
            try
            {
                _arcDbContext.RemoveArcUser(sql: sql, parameters: parameters);
                var result = parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_FoiAlterado).Value;

                return (bool)result;
            }
            catch (Exception ex)
            {
                parameters.FirstOrDefault(s => s.ParameterName == Constants.cst_Erro).Value = ex.Message;
                return false;
            }
        }

        public IARCDbContext GetContext() => _arcDbContext;

        private IQueryable<User> AddUserRelatedData(IQueryable<User> users)
        {
            return users
                .Include(u => u.UserGroupUsers)
                .ThenInclude(ugu => ugu.UserGroup)
                .Include(u => u.UserSubstitutions)
                .ThenInclude(usu => usu.SubstituteUser);
        }
    }
}
