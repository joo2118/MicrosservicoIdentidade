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
        Task<IReadOnlyCollection<TEntity>> GetAll(int? page, int? pageSize);

        void BeginTransaction();
        void DefineTransaction(bool successfulResult);

        void ConfigureParametersToCreateUpdate(out List<SqlParameter> parameters, string nome, DateTime vencimento, string createdUserId, string content, int classe, string prefixo);
        void ConfigureParametersToRemove(out List<SqlParameter> parameters, string userId);
        bool CreateUpdateItemDirectory(string sql, List<SqlParameter> parameters);
        bool RemoveItemDirectory(string sql, List<SqlParameter> parameters);

        IARCDbContext GetContext();
    }
}