using Ws_Server_with_gpt;
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHttpClient("BackendClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7286/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add MVC services to the container
builder.Services.AddControllers();
var app = builder.Build();

app.UseWebSockets();

// Register WebSocket handler middleware
app.UseMiddleware<WebSocketHandler>();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
