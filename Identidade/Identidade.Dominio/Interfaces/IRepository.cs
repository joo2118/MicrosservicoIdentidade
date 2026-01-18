using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identidade.Dominio.Interfaces
{
    public interface IReadOnlyRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetById(string entityId);
        Task<TEntity> GetByName(string entityName);
        Task<IReadOnlyCollection<TEntity>> GetAll();
        void BeginTransaction();
        void DefineTransaction(bool successfulResult);
        void ConfigureParametersToCreateUpdate(out List<SqlParameter> parameters, string nome, DateTime vencimento, string createdUserId, string content, int classe, string prefixo);
        void ConfigureParametersToRemove(out List<SqlParameter> parameters, string userId);
        bool CreateUpdateItemDirectory(string sql, List<SqlParameter> parameters);
        bool RemoveItemDirectory(string sql, List<SqlParameter> parameters);
        IARCDbContext GetContext();
    }

    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
    {
        Task<TEntity> Create(TEntity entity);
        Task<TEntity> Update(TEntity entity);
        Task<bool> Remove(string entityId);

        Task<string> RemoveByName(string userGroupName);
    }
}
