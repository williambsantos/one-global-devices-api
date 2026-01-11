using Microsoft.Data.SqlClient;

namespace OneGlobalDevicesApi.Infra.SQLServer.Connections
{
    public interface IDatabaseConnection
    {
        Task<SqlConnection> CreateSqlConnectionAsync(CancellationToken cancellationToken);
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly ILogger<DatabaseConnection> _logger;
        private readonly string _connectionString;
        public DatabaseConnection(string connectionString, ILogger<DatabaseConnection> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<SqlConnection> CreateSqlConnectionAsync(CancellationToken cancellationToken)
        {
            const string logPrefix = nameof(CreateSqlConnectionAsync) + ". ";

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
    }
}