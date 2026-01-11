using Dapper;
using OneGlobalDevicesApi.Domain.Entities;
using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using OneGlobalDevicesApi.Infra.SQLServer.Constants;
using System.Data;
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

        public async Task SaveAsync(DeviceEntity entity, CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(DeviceSqlServerRepository)}.{nameof(SaveAsync)}. Id: {entity?.Id}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                try
                {
                    using var connection = await _databaseConnection.CreateSqlConnectionAsync(cancellationToken);

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
                        CreationTime = entity.CreationTime

                    };

                    var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        commandTimeout: 30,
                        commandType: CommandType.Text,
                        cancellationToken: cancellationToken
                    ));
                    return id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{logPrefix}Error at inserted Device at database. " +
                        "Message: {message}. " +
                        "ID: {clientCode}",
                        logPrefix,
                        ex.Message,
                        entity.Id
                    );
                    throw;
                }
            });
        }

        public async Task UpdateAsync(DeviceEntity entity, CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(DeviceSqlServerRepository)}.{nameof(UpdateAsync)}. Id: {entity?.Id}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                try
                {
                    using var connection = await _databaseConnection.CreateSqlConnectionAsync(cancellationToken);

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
                        Name = entity.Name,
                        Brand = entity.Brand,
                        Id = entity.Id,
                    };

                    var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        commandTimeout: 30,
                        commandType: CommandType.Text,
                        cancellationToken: cancellationToken
                    ));
                    return id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{logPrefix}Error at update Device at database. " +
                        "Message: {message}. " +
                        "ID: {clientCode}",
                        logPrefix,
                        ex.Message,
                        entity.Id
                    );
                    throw;
                }
            });
        }

        public async Task DeleteAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(DeviceSqlServerRepository)}.{nameof(UpdateAsync)}. Id: {deviceId}. ";

            await LogActionAsync(logPrefix, async () =>
            {
                try
                {
                    using var connection = await _databaseConnection.CreateSqlConnectionAsync(cancellationToken);

                    var sql = $@"
DELETE {TableContants.DeviceTableName} 
WHERE Id = @Id
";
                    var parameters = new
                    {
                        Id = deviceId,
                    };

                    var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                        commandText: sql,
                        parameters: parameters,
                        commandTimeout: 30,
                        commandType: CommandType.Text,
                        cancellationToken: cancellationToken
                    ));
                    return id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{logPrefix}Error at update Device at database. " +
                        "Message: {message}. " +
                        "ID: {clientCode}",
                        logPrefix,
                        ex.Message,
                        deviceId
                    );
                    throw;
                }
            });
        }

        #endregion

        #region Fetch Operations

        public async Task<IEnumerable<DeviceEntity>> FetchAllAsync(CancellationToken cancellationToken = default)
        {
            var list = await InternalFetchByAsync(
                id: null,
                brand: null,
                state: null,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<IEnumerable<DeviceEntity>> FetchAllByBrandAsync(string deviceBrand, CancellationToken cancellationToken = default)
        {
            var list = await InternalFetchByAsync(
                id: null,
                brand: deviceBrand,
                state: null,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<IEnumerable<DeviceEntity>> FetchAllByStateAsync(DeviceStateEnum deviceState, CancellationToken cancellationToken = default)
        {
            var list = await InternalFetchByAsync(
                id: null,
                brand: null,
                state: deviceState,
                cancellationToken: cancellationToken
            );

            return list ?? [];
        }

        public async Task<DeviceEntity?> FetchByIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
        {
            var list = await InternalFetchByAsync(
                id: deviceId,
                brand: null,
                state: null,
                cancellationToken: cancellationToken
            );

            return list?.FirstOrDefault();
        }

        private async Task<IEnumerable<DeviceEntity>> InternalFetchByAsync(
            Guid? id,
            string? brand,
            DeviceStateEnum? state,
            CancellationToken cancellationToken = default)
        {
            string logPrefix = $"{nameof(DeviceSqlServerRepository)}.{nameof(InternalFetchByAsync)}. ";

            using var connection = await _databaseConnection.CreateSqlConnectionAsync(cancellationToken);

            var sql = $@"
SELECT
    [Id] as {nameof(DeviceEntity.Id)},
    [Name] as {nameof(DeviceEntity.Name)},
    [Brand] as {nameof(DeviceEntity.Brand)},
    [State] as {nameof(DeviceEntity.State)},
    [CreationTime] as {nameof(DeviceEntity.CreationTime)},
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
                State = state.HasValue ? (char?)state.Value : null
            };

            IEnumerable<DeviceEntity> list = await connection.QueryAsync<DeviceEntity>(new CommandDefinition(
                commandText: sql,
                parameters: parameters,
                commandTimeout: 30,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken
            ));

            return list ?? [];
        }

        #endregion
    }
}
