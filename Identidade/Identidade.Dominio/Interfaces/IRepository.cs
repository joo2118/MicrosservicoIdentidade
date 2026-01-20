using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identidade.Dominio.Interfaces
{

    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
    {
        Task<TEntity> Create(TEntity entity);
        Task<TEntity> Update(TEntity entity);
        Task<bool> Remove(string entityId);

        Task<string> RemoveByName(string userGroupName);
    }
}
