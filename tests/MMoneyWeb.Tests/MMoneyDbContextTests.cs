using MMoneyWeb.Web.Data;

namespace MMoneyWeb.Tests;

public class MMoneyDbContextTests
{
    [Fact]
    public void MMoneyDbContext_Type_IsDefinedInWebAssembly()
    {
        Assert.Equal("MMoneyWeb.Web", typeof(MMoneyDbContext).Assembly.GetName().Name);
    }
}
