using Microsoft.Data.SqlClient;
using OneGlobalDevicesApi.Domain.Repositories;
using System.Data.Common;

namespace OneGlobalDevicesApi.Infra.SQLServer.Connections
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly ILogger<DatabaseConnection> _logger;
        private readonly string _connectionString;
        public DatabaseConnection(string connectionString, ILogger<DatabaseConnection> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
        {
            const string logPrefix = nameof(CreateConnectionAsync) + ". ";

            try
            {
                var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                _logger.LogDebug("{logPrefix}Create SQL Server connection with success", logPrefix);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{logPrefix}Error to open a SQL Server Connection. " +
                    "Message: {message}", 
                    logPrefix, 
                    ex.Message
                );
                throw;
            }
        }

        public async Task<DatabaseWork> CreateConnectionAndTransactionAsync(CancellationToken cancellationToken)
        {
            const string logPrefix = nameof(CreateConnectionAndTransactionAsync) + ". ";

            try
            {
                var connection = await this.CreateConnectionAsync(cancellationToken);
                var transaction = await connection.BeginTransactionAsync(cancellationToken);

                return new DatabaseWork(connection, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{logPrefix}Error to open a SQL Server Connection. " +
                    "Message: {message}",
                    logPrefix,
                    ex.Message
                );
                throw;
            }
        }
    }
}