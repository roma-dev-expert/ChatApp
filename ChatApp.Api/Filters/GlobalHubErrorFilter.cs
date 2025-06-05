using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Filters
{
    public class GlobalHubErrorFilter : IHubFilter
    {
        private readonly ILogger<GlobalHubErrorFilter> _logger;

        public GlobalHubErrorFilter(ILogger<GlobalHubErrorFilter> logger)
        {
            _logger = logger;
        }

        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in method {Method} of hub {Hub} for connection {ConnectionId}",
                    invocationContext.HubMethodName,
                    invocationContext.Hub.GetType().Name,
                    invocationContext.Context.ConnectionId);

                string errorMessage = ex switch
                {
                    UnauthorizedAccessException _ => "Access denied.",
                    ArgumentException _ => "Invalid argument provided.",
                    _ => "An error occurred while processing your request."
                };

                await invocationContext.Hub.Clients.Caller.SendAsync("ReceiveError", errorMessage);
                throw;
            }
        }

        public async ValueTask OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, ValueTask> next)
        {
            _logger.LogInformation("Connection {ConnectionId} connected.", context.Context.ConnectionId);
            await next(context);
        }

        public async ValueTask OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, ValueTask> next)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "Connection {ConnectionId} disconnected due to an error.", context.Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Connection {ConnectionId} disconnected.", context.Context.ConnectionId);
            }
            await next(context, exception);
        }

    }
}