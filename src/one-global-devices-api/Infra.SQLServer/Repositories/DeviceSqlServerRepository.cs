using Dapper;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using OneGlobalDevicesApi.Infra.SQLServer.Constants;
using System.Data;
using System.Data.Common;
using static Dapper.SqlMapper;

namespace OneGlobalDevicesApi.Infra.SQLServer.Repositories
{
    public class DeviceSqlServerRepository : BaseRepository<DeviceSqlServerRepository>, IDeviceRepository
    {
        private readonly IDatabaseConnection _databaseConnection;

        public DeviceSqlServerRepository(
            ILogger<DeviceSqlServerRepository> logger,
            IDatabaseConnection databaseConnection)
            : base(logger)
        {
            _databaseConnection = databaseConnection;
        }

        #region Save | Update | Delete

        public async Task SaveAsync(DeviceEntity entity,
            CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(SaveAsync)}. Id: {entity?.Id}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                ArgumentNullException.ThrowIfNull(entity);

                using var connection = await _databaseConnection.CreateConnectionAsync(cancellationToken);

                var sql = $@"
INSERT INTO {TableContants.DeviceTableName} 
([Id], [Name], [Brand], [State], [CreationTime])
VALUES (@Id, @Name, @Brand, @State,@CreationTime);
";

                var parameters = new
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Brand = entity.Brand,
                    State = entity.State.ToString(),
                    CreationTime = entity.CreationTime
                };

                var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
                    commandText: sql,
                    parameters: parameters,
                    commandTimeout: 30,
                    commandType: CommandType.Text,
                    cancellationToken: cancellationToken
                ));
                return rowsAffected;
            });
        }

        public async Task UpdateAsync(DeviceEntity entity,
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(UpdateAsync)}. Id: {entity?.Id}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                ArgumentNullException.ThrowIfNull(connection);

                var sql = $@"
UPDATE {TableContants.DeviceTableName} 
SET
    [Name]  = @Name,
    [Brand] = @Brand,
    [State] = @State
WHERE Id = @Id
";

                var parameters = new
                {
                    Name = entity?.Name ?? string.Empty,
                    Brand = entity?.Brand ?? string.Empty,
                    Id = entity?.Id ?? Guid.Empty,
                    State = entity?.State.ToString() ?? string.Empty
                };

                var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
                    commandText: sql,
                    parameters: parameters,
                    transaction: transaction,
                    commandTimeout: 30,
                    commandType: CommandType.Text,
                    cancellationToken: cancellationToken
                ));
                return rowsAffected;
            });
        }

        public async Task DeleteAsync(Guid deviceId,
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(UpdateAsync)}. Id: {deviceId}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                if (connection == null)
                    throw new ArgumentNullException(nameof(connection));

                var sql = $@"
DELETE {TableContants.DeviceTableName} 
WHERE Id = @Id
";
                var parameters = new
                {
                    Id = deviceId,
                };

                var rowsAffected = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    commandText: sql,
                    parameters: parameters,
                    transaction: transaction,
                    commandTimeout: 30,
                    commandType: CommandType.Text,
                    cancellationToken: cancellationToken
                ));
                return rowsAffected;
            });
        }

        #endregion

        #region Fetch Operations

        public async Task<IEnumerable<DeviceEntity>> FetchAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = await _databaseConnection.CreateConnectionAsync(cancellationToken);

            var list = await InternalFetchByAsync(
                id: null,
                brand: null,
                state: null,
                connection: connection,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<IEnumerable<DeviceEntity>> FetchAllByBrandAsync(string deviceBrand, CancellationToken cancellationToken = default)
        {
            using var connection = await _databaseConnection.CreateConnectionAsync(cancellationToken);

            var list = await InternalFetchByAsync(
                id: null,
                brand: deviceBrand,
                state: null,
                connection: connection,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<IEnumerable<DeviceEntity>> FetchAllByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken = default)
        {
            using var connection = await _databaseConnection.CreateConnectionAsync(cancellationToken);

            var list = await InternalFetchByAsync(
                id: null,
                brand: null,
                state: deviceState,
                connection: connection,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<DeviceEntity?> FetchByIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            using var connection = await _databaseConnection.CreateConnectionAsync(cancellationToken);

            var list = await InternalFetchByAsync(
                id: deviceId,
                brand: null,
                state: null,
                connection: connection,
                cancellationToken: cancellationToken
            );

            return list?.FirstOrDefault();
        }

        public async Task<DeviceEntity?> FetchByIdAsync(Guid deviceId,
            DbConnection connection, DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            var list = await InternalFetchByAsync(
                id: deviceId,
                brand: null,
                state: null,
                connection: connection,
                transaction: transaction,
                cancellationToken: cancellationToken
            );

            return list?.FirstOrDefault();
        }

        private async Task<IEnumerable<DeviceEntity>> InternalFetchByAsync(
            Guid? id,
            string? brand,
            DeviceStateEnum? state,
            DbConnection connection,
            DbTransaction? transaction = default,
            CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(InternalFetchByAsync)}. ";

            var sql = $@"
SELECT
    [Id] as {nameof(DeviceEntity.Id)},
    [Name] as {nameof(DeviceEntity.Name)},
    [Brand] as {nameof(DeviceEntity.Brand)},
    [State] as {nameof(DeviceEntity.State)},
    [CreationTime] as {nameof(DeviceEntity.CreationTime)}
FROM {TableContants.DeviceTableName}
WHERE 1 = 1
AND (@Id IS NULL OR Id = @Id)
AND (@Brand IS NULL OR Brand = @Brand)
AND (@State IS NULL OR State = @State)
ORDER BY [CreationTime] ASC;";

            var parameters = new
            {
                Id = id,
                Brand = brand,
                State = state?.ToString()
            };

            IEnumerable<DeviceEntity> list = await connection.QueryAsync<DeviceEntity>(new CommandDefinition(
                commandText: sql,
                parameters: parameters,
                commandTimeout: 30,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken,
                transaction: transaction
            ));

            return list ?? [];
        }

        #endregion
    }
}
