using Microsoft.EntityFrameworkCore;
using Identidade.Dominio.Interfaces;
using System.Threading.Tasks;

namespace Identidade.Dominio.Helpers
{
    public interface IUpdateConcurrencyResolver
    {
        Task SaveChangesSafe();
    }

    public class UpdateConcurrencyResolver : IUpdateConcurrencyResolver
    {
        private readonly IARCDbContext _arcDbContext;

        public UpdateConcurrencyResolver(IARCDbContext arcDbContext)
        {
            _arcDbContext = arcDbContext;
        }

        public async Task SaveChangesSafe()
        {
            bool saved = false;
            while (!saved)
            {
                try
                {
                    await _arcDbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    foreach (var entry in e.Entries)
                    {
                        // Refreshing original values to bypass next concurrency check
                        var databaseValues = entry.GetDatabaseValues();
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
            }
        }
    }
}
