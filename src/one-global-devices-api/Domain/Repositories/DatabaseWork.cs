using System.Data.Common;

namespace OneGlobalDevicesApi.Domain.Repositories
{
    public class DatabaseWork : IDisposable
    {
        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }

        private bool _disposed = false;

        public DatabaseWork(DbConnection connection, DbTransaction transaction)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Transaction?.Dispose();
                Connection?.Dispose();
            }

            _disposed = true;
        }

        ~DatabaseWork()
        {
            Dispose(false);
        }
    }
}
