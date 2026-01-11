using System.Data;
using System.Data.Common;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public interface IDatabaseConnection
    {
        Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken);
        Task<DatabaseWork> CreateConnectionAndTransactionAsync(CancellationToken cancellationToken);
    }
}
