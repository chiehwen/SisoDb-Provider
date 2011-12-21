using SisoDb.Dac;
using SisoDb.DbSchema;
using SisoDb.Providers;
using SisoDb.Structures;

namespace SisoDb.Sql2008
{
    public class Sql2008UnitOfWork : DbUnitOfWork
    {
        protected internal Sql2008UnitOfWork(
			ISisoDatabase db,
			IDbClient dbClientTransactional,
			IDbClient dbClientNonTransactional,
			IDbSchemaManager dbSchemaManager,
			IIdentityStructureIdGenerator identityStructureIdGenerator,
			ISqlStatements sqlStatements)
            : base(db, dbClientTransactional, dbClientNonTransactional, dbSchemaManager, identityStructureIdGenerator, sqlStatements)
        {
        }
    }
}