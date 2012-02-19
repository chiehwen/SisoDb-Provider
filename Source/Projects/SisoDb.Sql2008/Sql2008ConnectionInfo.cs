﻿using System;
using System.Data.SqlClient;
using SisoDb.Resources;

namespace SisoDb.Sql2008
{
    [Serializable]
    public class Sql2008ConnectionInfo : SisoConnectionInfo
    {
        private readonly string _dbName;

        public override StorageProviders ProviderType
        {
            get { return StorageProviders.Sql2008; }
        }

        public override string DbName
        {
            get { return _dbName; }
        }

        public Sql2008ConnectionInfo(string connectionStringOrName)
            : this(ConnectionString.Get(connectionStringOrName))
        { }

        public Sql2008ConnectionInfo(IConnectionString connectionString)
            : base(connectionString)
        {
            _dbName = ExtractDbName(ClientConnectionString);

            if (string.IsNullOrWhiteSpace(_dbName))
                throw new SisoDbException(ExceptionMessages.ConnectionInfo_MissingName);
        }

        protected override IConnectionString OnFormatServerConnectionString(IConnectionString connectionString)
        {
            var cnString = base.OnFormatServerConnectionString(connectionString);
            var cnStringBuilder = new SqlConnectionStringBuilder(cnString.PlainString);
            cnStringBuilder.InitialCatalog = string.Empty;

            return cnString.ReplacePlain(cnStringBuilder.ConnectionString);
        }

        private string ExtractDbName(IConnectionString connectionString)
        {
            var cnStringBuilder = new SqlConnectionStringBuilder(connectionString.PlainString);
            return cnStringBuilder.InitialCatalog;
        }
    }
}