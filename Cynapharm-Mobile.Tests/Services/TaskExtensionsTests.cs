using Cynapharm_Mobile.Services;

namespace Cynapharm_Mobile.Tests.Services;

public class TaskExtensionsTests
{
    [Fact]
    public async Task SafeFireAndForget_CompletesWithoutException_WhenTaskSucceeds()
    {
        var completed = false;
        Task.Run(() => { completed = true; }).SafeFireAndForget();
        await Task.Delay(50);
        Assert.True(completed);
    }

    [Fact]
    public async Task SafeFireAndForget_InvokesOnError_WhenTaskThrows()
    {
        Exception? caught = null;
        var ex = new InvalidOperationException("boom");

        Task.FromException(ex).SafeFireAndForget(e => caught = e);
        await Task.Delay(50);

        Assert.Same(ex, caught);
    }

    [Fact]
    public async Task SafeFireAndForget_DoesNotThrow_WhenOnErrorCallbackIsNull()
    {
        // An exception in the task must not propagate when no callback is supplied
        var exception = await Record.ExceptionAsync(async () =>
        {
            Task.FromException(new Exception("silent")).SafeFireAndForget();
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }
}
