[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DALTests.TestHelpers;

public static class TestConnectionStringHelper
{
    public const string ConnectionString = "Data Source=DESKTOP-63A2F9K;Initial Catalog=ForeignLiteratureSection_test;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";
}
