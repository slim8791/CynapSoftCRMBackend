using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Tests.Models;

public class OrderTests
{
    [Fact]
    public void NumeroCommande_PadsIdToFiveDigits()
    {
        var order = new Order { Id = 1 };
        Assert.Equal("CMD-00001", order.NumeroCommande);
    }

    [Fact]
    public void NumeroCommande_HandlesExactlyFiveDigitId()
    {
        var order = new Order { Id = 12345 };
        Assert.Equal("CMD-12345", order.NumeroCommande);
    }

    [Fact]
    public void NumeroCommande_HandlesLargeIdWithoutTruncation()
    {
        var order = new Order { Id = 999999 };
        Assert.Equal("CMD-999999", order.NumeroCommande);
    }
}
