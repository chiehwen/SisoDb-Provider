using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using NCore.Collections;
using SisoDb.Providers;
using SisoDb.Querying.Sql;
using SisoDb.Resources;
using SisoDb.Structures;

namespace SisoDb.Querying
{
    public abstract class DbQueryGenerator : IDbQueryGenerator
    {
        protected readonly ISqlStatements SqlStatements;
        protected readonly SqlExpressionBuilder SqlExpressionBuilder;

        protected DbQueryGenerator(ISqlStatements sqlStatements)
        {
            Ensure.That(sqlStatements, "sqlStatements").IsNotNull();

            SqlStatements = sqlStatements;
            SqlExpressionBuilder = new SqlExpressionBuilder();
        }

        public SqlQuery GenerateQuery(IQuery query)
        {
            Ensure.That(query, "query").IsNotNull();

            return CreateSqlQuery(query);
        }

        public SqlQuery GenerateQueryReturningStrutureIds(IQuery query)
        {
			Ensure.That(query, "query").IsNotNull();

			if (!query.HasWhere || (query.HasTakeNumOfStructures || query.HasIncludes || query.HasSortings || query.HasPaging))
                throw new ArgumentException(ExceptionMessages.DbQueryGenerator_GenerateQueryReturningStrutureIds);

			return CreateSqlQueryReturningStructureIds(query);
        }

		protected abstract SqlQuery CreateSqlQuery(IQuery query);

		protected abstract SqlQuery CreateSqlQueryReturningStructureIds(IQuery query);

		protected virtual string GenerateStartString(IQuery query, ISqlExpression sqlExpression)
        {
            return string.Empty;
        }

		protected virtual string GenerateEndString(IQuery query, ISqlExpression sqlExpression)
        {
            return string.Empty;
        }

		protected virtual string GenerateTakeString(IQuery query)
        {
			if (!query.HasTakeNumOfStructures || query.HasPaging)
                return string.Empty;

			return string.Format("top ({0})", query.TakeNumOfStructures);
        }

		protected abstract string GeneratePagingString(IQuery query, ISqlExpression sqlExpression);

		protected virtual string GenerateOrderByMembersString(IQuery queryCommand, ISqlExpression sqlExpression)
        {
            var sortings = sqlExpression.SortingMembers.ToList();

            return sortings.Count == 0
                ? string.Empty
                : string.Join(", ", sortings.Select(sorting => string.Format("min(mem{0}.[{1}]) mem{0}", sorting.Index, sorting.IndexStorageColumnName)));
        }

		protected virtual string GenerateOrderByString(IQuery query, ISqlExpression sqlExpression)
        {
            var sortings = sqlExpression.SortingMembers.ToList();

            return sortings.Count == 0
                ? string.Empty
                : string.Join(", ", sortings.Select(sorting => string.Format("mem{0} {1}", sorting.Index, sorting.Direction)));
        }

		protected virtual string GenerateWhereAndSortingJoins(IQuery query, ISqlExpression sqlExpression)
        {
            var wheres = sqlExpression.WhereMembers.ToList();
            var sortings = sqlExpression.SortingMembers.ToList();

			var indexesTableName = query.StructureSchema.GetIndexesTableName();

            var joins = new List<string>(wheres.Count + sortings.Count);

            const string joinFormat = "inner join [{0}] mem{1} on mem{1}.[StructureId] = s.[StructureId] and mem{1}.[MemberPath] = '{2}'";

            if (wheres.Count > 0)
            {
                joins.AddRange(wheres.Select(where =>
                    string.Format(joinFormat,
                    indexesTableName,
                    where.Index,
                    where.MemberPath)));
            }

            if (sortings.Count > 0)
            {
                joins.AddRange(sortings.Select(sorting =>
                    string.Format(joinFormat,
                    indexesTableName,
                    sorting.Index,
                    sorting.MemberPath)));
            }

            return string.Join(" ", joins.Distinct());
        }

        protected virtual string GenerateWhereCriteriaString(ISqlExpression sqlExpression)
        {
            return sqlExpression.WhereCriteria.CriteriaString;
        }

		protected virtual string GenerateMatchingIncludesJoins(IQuery query, ISqlExpression sqlExpression)
        {
            var includes = sqlExpression.Includes.ToList();
            if (includes.Count == 0)
                return string.Empty;

            var indexesJoinString = string.Format("inner join [{0}] si on si.[StructureId] = s.[StructureId] and si.[MemberPath] in ({1})",
				query.StructureSchema.GetIndexesTableName(),
                string.Join(", ", includes.Select(inc => string.Format("'{0}'", inc.MemberPathReference))));

            const string joinFormat = "left join [{0}] cs{1} on cs{1}.[StructureId] = si.[{2}] and si.[MemberPath] = '{3}'";

            return string.Join(" ", new [] { indexesJoinString }.MergeWith(includes.Select(inc => string.Format(joinFormat, inc.TableName, inc.Index, inc.IndexValueColumnName, inc.MemberPathReference))));
        }

        protected virtual string GenerateIncludesJoins(ISqlExpression sqlExpression)
        {
            var includes = sqlExpression.Includes.ToList();

            const string joinFormat = "left join [{0}] cs{1} on cs{1}.[RowId] = rs.[{2}RowId]";

            return includes.Count == 0
                ? string.Empty
                : string.Join(" ", includes.Select(inc => string.Format(joinFormat, inc.TableName, inc.Index, inc.ObjectReferencePath)));
        }

        protected virtual string GenerateIncludedJsonMembersString(ISqlExpression sqlExpression)
        {
            var includes = sqlExpression.Includes.ToList();

            return includes.Count == 0
                ? string.Empty
                : string.Join(", ", includes.Select(inc => string.Format("cs{0}.[Json] [{1}Json]", inc.Index, inc.ObjectReferencePath)));
        }

        protected virtual string GenerateIncludedRowIdsString(ISqlExpression sqlExpression)
        {
            var includes = sqlExpression.Includes.ToList();

            return includes.Count == 0
                ? string.Empty
                : string.Join(", ", includes.Select(inc => string.Format("min(cs{0}.[RowId]) [{1}RowId]", inc.Index, inc.ObjectReferencePath)));
        }
    }
}