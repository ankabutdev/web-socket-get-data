using System.Net.WebSockets;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri("wss://localhost:7061"), CancellationToken.None);
        while (true)
        {
            Console.WriteLine("Connected!");

            Console.Write("UserId: ");
            var userId = Console.ReadLine(); // Replace with the actual userId
            var sendBytes = Encoding.UTF8.GetBytes(userId);
            var sendBuffer = new ArraySegment<byte>(sendBytes);

            await client.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent: {userId}");

            var receiveBuffer = new byte[1024];
            var receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
            var receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
            Console.WriteLine($"Received: {receivedMessage}");

            if (client.State == WebSocketState.Closed || client.State == WebSocketState.Aborted)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                Console.WriteLine("Closed!");
                break;
            }
        }
    }
}
