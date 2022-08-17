using fbognini.Application.Persistence;
using fbognini.Infrastructure.Common;
using fbognini.Infrastructure.Multitenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;

namespace fbognini.Infrastructure.Persistence.ConnectionString
{

    public class ConnectionStringValidator : IConnectionStringValidator
    {
        private readonly DatabaseSettings dbSettings;
        private readonly ILogger<ConnectionStringValidator> logger;

        public ConnectionStringValidator(IOptions<DatabaseSettings> dbSettings, ILogger<ConnectionStringValidator> logger)
        {
            this.dbSettings = dbSettings.Value;
            this.logger = logger;
        }

        public bool TryValidate(string connectionString, string? dbProvider = null)
        {
            if (string.IsNullOrWhiteSpace(dbProvider))
            {
                dbProvider = dbSettings.DBProvider;
            }

            try
            {
                switch (dbProvider?.ToLowerInvariant())
                {
                    //case DbProviderKeys.Npgsql:
                    //    var postgresqlcs = new NpgsqlConnectionStringBuilder(connectionString);
                    //    break;

                    //case DbProviderKeys.MySql:
                    //    var mysqlcs = new MySqlConnectionStringBuilder(connectionString);
                    //    break;

                    case DbProviderKeys.SqlServer:
                        var mssqlcs = new SqlConnectionStringBuilder(connectionString);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connection String Validation Exception");
                return false;
            }
        }
    }

}