using Machine.Specifications;
using SisoDb.Testing;

namespace SisoDb.Specifications.Database
{
    class DeleteIfExists
    {
        [Subject(typeof (ISisoDatabase), "Delete if exists")]
        public class when_database_exists
        {
            Establish context = () =>
            {
                _testContext = TestContextFactory.CreateTemp();
                _testContext.Database.CreateIfNotExists();
            };

            Because of =
                () => _testContext.Database.DeleteIfExists();

            It should_get_dropped = 
                () => _testContext.Database.Exists().ShouldBeFalse();

            private static ITestContext _testContext;
        }
    }
}