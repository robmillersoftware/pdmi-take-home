using CryptidCare.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CryptidCare.Api.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    private const string ValidKey = "test-key";

    private static ApiKeyMiddleware BuildMiddleware(RequestDelegate next) =>
        new(next, new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiKey"] = ValidKey })
            .Build());

    private static DefaultHttpContext BuildContext(string? apiKey)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (apiKey is not null)
            context.Request.Headers["X-Api-Key"] = apiKey;
        return context;
    }

    [Fact]
    public async Task ValidApiKey_CallsNext()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(BuildContext(ValidKey));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task MissingApiKey_Returns401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = BuildContext(apiKey: null);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task WrongApiKey_Returns401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = BuildContext("wrong-key");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }
}
