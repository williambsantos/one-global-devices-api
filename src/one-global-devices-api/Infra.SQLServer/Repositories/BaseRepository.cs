namespace OneGlobalDevicesApi.Infra.SQLServer.Repositories
{
    public class BaseRepository<TClass>
    {
        protected readonly ILogger<TClass> _logger;

        protected BaseRepository(ILogger<TClass> logger)
        {
            _logger = logger;
        }

        protected async Task<int> LogActionAsync(string logPrefix, Func<Task<int>> func)
        {
            try
            {
                _logger.LogInformation("{logPrefix} START", logPrefix);

                int rowsAffected = await func();

                _logger.LogInformation("{logPrefix} rows affected: {rowsAffected}", logPrefix, rowsAffected);

                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{logPrefix} ERROR {errorMessage}", logPrefix, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("{logPrefix} FINISH", logPrefix);
            }
        }     
    }
}

