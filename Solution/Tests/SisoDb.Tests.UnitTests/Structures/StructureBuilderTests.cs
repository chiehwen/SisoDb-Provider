using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using SisoDb.Structures;
using SisoDb.Structures.Schemas;

namespace SisoDb.Tests.UnitTests.Structures
{
    [TestFixture]
    public class StructureBuilderTests : UnitTestBase
    {
        private StructureBuilder _builder;

        protected override void OnTestInitialize()
        {
            _builder = new StructureBuilder(
                SisoEnvironment.Resources.ResolveJsonSerializer(),
                new SisoIdFactory(),
                new StructureIndexesFactory(SisoEnvironment.Formatting.StringConverter));
        }

        [Test]
        public void CreateStructure_WhenItemWithNoEnumerable_ReturnsIndexWithStringValue()
        {
            var schema = StructureSchemaTestFactory.CreateSchema<WithNoArray>();
            var item = new WithNoArray { Value = "A" };

            var structure = _builder.CreateStructure(item, schema);

            var indexes = structure.Indexes.ToList();
            Assert.AreEqual("A", indexes[0].Value);
        }

        [Test]
        public void CreateStructure_WhenItemWithEnumerableWithOneElement_ReturnsIndexWithStringsAsOneValuedDelimitedString()
        {
            var schema = StructureSchemaTestFactory.CreateSchema<WithArray>();
            var item = new WithArray { Values = new[] { "A" } };

            var structure = _builder.CreateStructure(item, schema);

            var indexes = structure.Indexes.ToList();
            Assert.AreEqual("<$A$>", indexes[0].Value);
        }

        [Test]
        public void CreateStructure_WhenItemWithEnumerableWithTwoDifferentElements_ReturnsIndexWithStringsAsTwoValuedDelimitedString()
        {
            var schema = StructureSchemaTestFactory.CreateSchema<WithArray>();
            var item = new WithArray { Values = new[] { "A", "B" } };

            var structure = _builder.CreateStructure(item, schema);

            var indexes = structure.Indexes.ToList();
            Assert.AreEqual("<$A$><$B$>", indexes[0].Value);
        }

        [Test]
        public void CreateStructure_WhenItemWithEnumerableWithTwoEqualElements_ReturnsIndexWithStringsAsOneValuedDelimitedString()
        {
            var schema = StructureSchemaTestFactory.CreateSchema<WithArray>();
            var item = new WithArray { Values = new[] { "A", "A" } };

            var structure = _builder.CreateStructure(item, schema);

            var indexes = structure.Indexes.ToList();
            Assert.AreEqual("<$A$>", indexes[0].Value);
        }

        [Test]
        public void CreateStructure_WhenItemWithByteArray_NoIndexShouldBeCreatedForByteArray()
        {
            var schema = StructureSchemaTestFactory.CreateSchema<WithBytes>();
            var item = new WithBytes { Bytes1 = BitConverter.GetBytes(242) };

            var structure = _builder.CreateStructure(item, schema);

            var indexes = structure.Indexes.ToList();
            Assert.AreEqual(1, indexes.Count);
            Assert.IsTrue(indexes[0].Name.StartsWith("DummyMember_"));
        }

        private class WithBytes
        {
            public Guid SisoId { get; set; }

            public int DummyMember { get; set; }

            public byte[] Bytes1 { get; set; }

            public IEnumerable<byte> Bytes2 { get; set; }

            public IList<byte> Bytes3 { get; set; }

            public List<byte> Bytes4 { get; set; }

            public ICollection<byte> Bytes5 { get; set; }

            public Collection<byte> Bytes6 { get; set; }
        }

        private class WithByte
        {
            public Guid SisoId { get; set; }

            public byte Byte { get; set; }
        }

        private class WithArray
        {
            public Guid SisoId { get; set; }

            public string[] Values { get; set; }
        }

        private class WithNoArray
        {
            public Guid SisoId { get; set; }

            public string Value { get; set; }
        }
    }
}