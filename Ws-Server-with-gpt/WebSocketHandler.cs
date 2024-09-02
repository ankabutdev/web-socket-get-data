using System.Net.WebSockets;
using System.Text;

namespace Ws_Server_with_gpt;

public class WebSocketHandler
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;

    public WebSocketHandler(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketAsync(webSocket);
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var userId = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Fetch user details from the backend
            var userDetails = await GetUserDetailsAsync(userId);

            // Send user details back to the client
            var responseMessage = Encoding.UTF8.GetBytes(userDetails);
            await webSocket.SendAsync(new ArraySegment<byte>(responseMessage, 0, responseMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private async Task<string> GetUserDetailsAsync(string userId)
    {
        var client = _httpClientFactory.CreateClient("BackendClient");
        var response = await client.GetAsync($"api/users/{userId}");
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            return "User not found!";
        }

        return await response.Content.ReadAsStringAsync();
    }
}
