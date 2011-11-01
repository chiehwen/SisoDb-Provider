﻿using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using SisoDb.Dac;
using SisoDb.Querying;
using SisoDb.Sql2008;
using SisoDb.Testing;

namespace SisoDb.Specifications.Sql2008.QueryEngine
{
    namespace NamedQueries
    {
        [Subject(typeof(Sql2008QueryEngine), "Named Query")]
        public class when_named_query_returns_no_result : SpecificationBase, ICleanupAfterEveryContextInAssembly
        {
            Establish context = () =>
            {
                TestContext = TestContextFactory.Create(StorageProviders.Sql2008);
                TestContext.DbHelper.CreateProcedure(@"create procedure [dbo].[" + ProcedureName + "] as begin select '{}' as Json where 1=2; end");
            };

            public void AfterContextCleanup()
            {
                TestContext.DbHelper.DropProcedure(ProcedureName);
            }

            Because of = () =>
            {
                var query = new NamedQuery(ProcedureName);
                _fetchedStructures = TestContext.Database.ReadOnce().NamedQuery<QueryGuidItem>(query).ToList();
            };

            It should_have_fetched_0_structures =
                () => _fetchedStructures.Count.ShouldEqual(0);

            private const string ProcedureName = "NamedQueryReturningNullTest";
            private static IList<QueryGuidItem> _fetchedStructures;
        }

        [Subject(typeof(Sql2008QueryEngine), "Named Query as Json")]
        public class when_named_query_returns_no_json_result : SpecificationBase, ICleanupAfterEveryContextInAssembly
        {
            Establish context = () =>
            {
                TestContext = TestContextFactory.Create(StorageProviders.Sql2008);
                TestContext.DbHelper.CreateProcedure(@"create procedure [dbo].[" + ProcedureName + "] as begin select '{}' as Json where 1=2; end");
            };

            public void AfterContextCleanup()
            {
                TestContext.DbHelper.DropProcedure(ProcedureName);
            }

            Because of = () =>
            {
                var query = new NamedQuery(ProcedureName);
                _fetchedStructures = TestContext.Database.ReadOnce().NamedQueryAsJson<QueryGuidItem>(query).ToList();
            };

            It should_have_fetched_0_structures =
                () => _fetchedStructures.Count.ShouldEqual(0);

            private const string ProcedureName = "NamedQueryAsJsonReturningNullTest";
            private static IList<string> _fetchedStructures;
        }

        [Subject(typeof(Sql2008QueryEngine), "Named Query")]
        public class when_named_query_with_parameters : SpecificationBase, ICleanupAfterEveryContextInAssembly
        {
            Establish context = () =>
            {
                TestContext = TestContextFactory.Create(StorageProviders.Sql2008);
                _structures = TestContext.Database.WriteOnce().InsertMany(QueryGuidItem.CreateFourItems<QueryGuidItem>());
                TestContext.DbHelper.CreateProcedure(@"create procedure [dbo].[" + ProcedureName + "] @minSortOrder int, @maxSortOrder int as begin select s.Json from [dbo].[QueryGuidItemStructure] as s inner join [dbo].[QueryGuidItemIndexes] as si on si.[StructureId] = s.[StructureId] where (si.[MemberPath]='SortOrder' and si.[IntegerValue] between @minSortOrder and @maxSortOrder) group by s.[StructureId], s.[Json] order by s.[StructureId]; end");
            };

            public void AfterContextCleanup()
            {
                TestContext.DbHelper.DropProcedure(ProcedureName);
            }

            Because of = () =>
            {
                var query = new NamedQuery(ProcedureName);
                query.Add(
                    new DacParameter("minSortOrder", _structures[1].SortOrder), 
                    new DacParameter("maxSortOrder", _structures[2].SortOrder));

                _fetchedStructures = TestContext.Database.ReadOnce().NamedQuery<QueryGuidItem>(query).ToList();
            };

            It should_have_fetched_two_structures =
                () => _fetchedStructures.Count.ShouldEqual(2);

            It should_have_fetched_the_two_middle_structures = () =>
            {
                _fetchedStructures[0].ShouldBeValueEqualTo(_structures[1]);
                _fetchedStructures[1].ShouldBeValueEqualTo(_structures[2]);
            };

            private const string ProcedureName = "NamedQueryTest";
            private static IList<QueryGuidItem> _structures;
            private static IList<QueryGuidItem> _fetchedStructures;
        }

        [Subject(typeof(Sql2008QueryEngine), "Named Query as Json")]
        public class when_named_query_with_parameters_returning_json : SpecificationBase, ICleanupAfterEveryContextInAssembly
        {
            Establish context = () =>
            {
                TestContext = TestContextFactory.Create(StorageProviders.Sql2008);
                _structures = TestContext.Database.WriteOnce().InsertMany(QueryGuidItem.CreateFourItems<QueryGuidItem>());
                TestContext.DbHelper.CreateProcedure(@"create procedure [dbo].[" + ProcedureName + "] @minSortOrder int, @maxSortOrder int as begin select s.Json from [dbo].[QueryGuidItemStructure] as s inner join [dbo].[QueryGuidItemIndexes] as si on si.[StructureId] = s.[StructureId] where (si.[MemberPath]='SortOrder' and si.[IntegerValue] between @minSortOrder and @maxSortOrder) group by s.[StructureId], s.[Json] order by s.[StructureId]; end");
            };

            public void AfterContextCleanup()
            {
                TestContext.DbHelper.DropProcedure(ProcedureName);
            }

            Because of = () =>
            {
                var query = new NamedQuery(ProcedureName);
                query.Add(
                    new DacParameter("minSortOrder", _structures[1].SortOrder),
                    new DacParameter("maxSortOrder", _structures[2].SortOrder));

                _fetchedStructures = TestContext.Database.ReadOnce().NamedQueryAsJson<QueryGuidItem>(query).ToList();
            };

            It should_have_fetched_two_structures =
                () => _fetchedStructures.Count.ShouldEqual(2);

            It should_have_fetched_the_two_middle_structures = () =>
            {
                _fetchedStructures[0].ShouldEqual(_structures[1].AsJson());
                _fetchedStructures[1].ShouldEqual(_structures[2].AsJson());
            };

            private const string ProcedureName = "NamedQueryAsJsonTest";
            private static IList<QueryGuidItem> _structures;
            private static IList<string> _fetchedStructures;
        }
    }
}