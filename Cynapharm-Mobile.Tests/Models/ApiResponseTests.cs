using Cynapharm_Mobile.Models.Common;

namespace Cynapharm_Mobile.Tests.Models;

public class ApiResponseTests
{
    [Fact]
    public void IsSuccess_IsTrueByDefault()
    {
        var response = new ApiResponse<string>();
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void Success_IsAliasForIsSuccess()
    {
        var response = new ApiResponse<string> { IsSuccess = false };
        Assert.False(response.Success);
    }

    [Fact]
    public void Data_IsAliasForResult()
    {
        var response = new ApiResponse<string> { Result = "hello" };
        Assert.Equal("hello", response.Data);
    }

    [Fact]
    public void Success_ReflectsIsSuccess_AfterChange()
    {
        var response = new ApiResponse<string> { IsSuccess = true };
        Assert.True(response.Success);

        response.IsSuccess = false;
        Assert.False(response.Success);
    }
}
