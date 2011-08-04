﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using SisoDb.Core;
using SisoDb.DbSchema;
using SisoDb.Providers;
using SisoDb.Sql2008.Dac;
using SisoDb.Structures.Schemas;

namespace SisoDb.Sql2008.DbSchema
{
    /// <summary>
    /// Delete records that represents an unique for a column that
    /// has been dropped in the key-value table for Uniques.
    /// </summary>
    public class SqlDbUniquesSchemaSynchronizer : IDbSchemaSynchronizer
    {
        private readonly SqlDbClient _dbClient;
        private readonly ISqlStatements _sqlStatements;

        public SqlDbUniquesSchemaSynchronizer(SqlDbClient dbClient)
        {
            _dbClient = dbClient;
            _sqlStatements = dbClient.SqlStatements;
        }

        public void Synchronize(IStructureSchema structureSchema)
        {
            var keyNamesToDrop = GetKeyNamesToDrop(structureSchema);

            if (keyNamesToDrop.Count > 0)
                DeleteRecordsMatchingKeyNames(structureSchema, keyNamesToDrop);
        }

        private void DeleteRecordsMatchingKeyNames(IStructureSchema structureSchema, IEnumerable<string> names)
        {
            var inString = string.Join(",", names.Select(n => "'" + n + "'"));
            var sql = _sqlStatements.GetSql("UniquesSchemaSynchronizer_DeleteRecordsMatchingKeyNames")
                .Inject(structureSchema.GetUniquesTableName(), UniqueStorageSchema.Fields.UqName.Name, inString);

            using (var cmd = _dbClient.CreateCommand(CommandType.Text, sql))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private IList<string> GetKeyNamesToDrop(IStructureSchema structureSchema)
        {
            var structureFields = new HashSet<string>(structureSchema.IndexAccessors.Select(iac => iac.Name));
            var keyNames = GetKeyNames(structureSchema);

            return keyNames.Where(kn => !structureFields.Contains(kn)).ToList();
        }

        private IEnumerable<string> GetKeyNames(IStructureSchema structureSchema)
        {
            var dbColumns = new List<string>();

            _dbClient.SingleResultSequentialReader(CommandType.Text,
                                                _sqlStatements.GetSql("UniquesSchemaSynchronizer_GetKeyNames")
                                                .Inject(
                                                    UniqueStorageSchema.Fields.UqName.Name, structureSchema.GetUniquesTableName()),
                                                    dr => dbColumns.Add(dr.GetString(0)));

            return dbColumns;
        }
    }
}