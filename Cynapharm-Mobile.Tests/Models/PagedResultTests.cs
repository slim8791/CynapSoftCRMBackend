using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Tests.Models;

public class PagedResultTests
{
    [Fact]
    public void HasMore_ReturnsTrue_WhenMorePagesExist()
    {
        // Page 1, size 10, total 25 → 10 loaded, 15 remain
        var result = new PagedResult<string> { Page = 1, PageSize = 10, TotalCount = 25 };
        Assert.True(result.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsFalse_WhenOnLastPage()
    {
        // Page 3, size 10, total 25 → 30 would be loaded, but only 25 exist
        var result = new PagedResult<string> { Page = 3, PageSize = 10, TotalCount = 25 };
        Assert.False(result.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsFalse_WhenPageTimeSizeExactlyEqualsTotalCount()
    {
        // Page 2, size 10, total 20 → exactly at the boundary
        var result = new PagedResult<string> { Page = 2, PageSize = 10, TotalCount = 20 };
        Assert.False(result.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsFalse_WhenTotalCountIsZero()
    {
        var result = new PagedResult<string> { Page = 1, PageSize = 10, TotalCount = 0 };
        Assert.False(result.HasMore);
    }
}
