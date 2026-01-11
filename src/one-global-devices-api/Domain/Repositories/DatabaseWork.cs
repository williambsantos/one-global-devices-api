using System.Data.Common;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public class DatabaseWork : IDisposable
    {
        public DbConnection Connection { get; set; }
        public DbTransaction Transaction { get; set; }

        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }
}
