using System;
using SisoDb.Structures;
using SisoDb.Structures.Schemas;

namespace SisoDb
{
    public interface ISessionEvents
    {
        /// <summary>
        /// Called when a session is disposed and committed.
        /// </summary>
        Action<ISisoDatabase, Guid> OnCommitted { set; }

        /// <summary>
        /// Called when a session is rolled back and disposed due to an failure.
        /// </summary>
        Action<ISisoDatabase, Guid> OnRolledback { set; }

        /// <summary>
        /// Called when an item has been inserted.
        /// </summary>
        Action<ISession, IStructureSchema, IStructure, object> OnInserted { set; }
        
        /// <summary>
        /// Called when an item has been updated.
        /// </summary>
        Action<ISession, IStructureSchema, IStructure, object> OnUpdated { set; }
        
        /// <summary>
        /// Called when delete has been performed on an id.
        /// </summary>
        Action<ISession, IStructureSchema, IStructureId> OnDeleted { set; }
        
        /// <summary>
        /// Called when delete has been performed by a query.
        /// </summary>
        Action<ISession, IStructureSchema, IQuery> OnDeletedByQuery { set; }
    }
}