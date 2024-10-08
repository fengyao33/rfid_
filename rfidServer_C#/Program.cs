using rfidServer_C_.Models;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using System;
using rfidServer_C_.AppDbContext;
using static rfidServer_C_.AppDbContext.Scw2022DbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connnetionString = builder.Configuration.GetConnectionString("DefaultConnectDB") ?? throw new NullReferenceException("No connection string in config");
var conntScw2022 = builder.Configuration.GetConnectionString("Scw2022") ?? throw new NullReferenceException("No connection string in config");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectDB")));
builder.Services.AddDbContext<Scw2022DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Scw2022")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin() 
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseWebSockets();

var clients = new ConcurrentDictionary<string, WebSocket>();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocket(webSocket, clients);
    }
    else
    {
        await next();
    }
});

app.MapControllers();

app.Run("http://192.168.61.104:3333");


async Task HandleWebSocket(WebSocket webSocket, ConcurrentDictionary<string, WebSocket> clients)
{
    var buffer = new byte[1024 * 4];
    string clientId = Guid.NewGuid().ToString();
    clients[clientId] = webSocket;

    try
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else
            {
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received message: {message}");

                var parseMessage = System.Text.Json.JsonSerializer.Deserialize<Socket>(message);
                var type = parseMessage?.Type; 
                var data = parseMessage?.Data;

                if (type == "register")
                {
                    var response = JsonSerializer.Serialize(new { Type = "registered", ClientId = clientId });
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (type == "Query")
                {
                    var queryData = JsonSerializer.Deserialize<QueryData>(data.ToString());

                    using (var scope = app.Services.CreateScope())
                    {
                        var scwContext = scope.ServiceProvider.GetRequiredService<Scw2022DbContext>();

                        try
                        {
                            var packInfoList = await scwContext.GetPackInfoByPalletNoAsync(queryData.TagID);

                            var responseData = JsonSerializer.Serialize(new
                            {
                                type = "queryResult",
                                code = 200,
                                data = packInfoList
                            }, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseData)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error occurred: {ex.Message}");
                        }

                    }


                }
                else if (type == "History")
                {
                    var historyData = JsonSerializer.Deserialize<HistoryData>(data.ToString());


                    using (var scope = app.Services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        //var historyRecords = await dbContext.TblRFIDHistory
                        //  .Where(h => h.TagID == historyData.TagID)
                        //  .ToListAsync();

                        var historyRecords = await dbContext.TblRFIDHistory
                        .Where(h => h.TagID == historyData.TagID && h.TagID != null)
                        .ToListAsync();

                        var responseData = JsonSerializer.Serialize(new
                        {
                            type = "historyRecorded",
                            code = 200,
                            data = historyRecords
                        }, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseData)), WebSocketMessageType.Text, true, CancellationToken.None);
                        //var historyRecord = new tblRFIDHistory
                        //{
                        //    Rid = Guid.NewGuid(),
                        //    TagID = historyData.TagID,
                        //    Location = historyData.Location,
                        //    Action = historyData.Action,
                        //    BDate = DateTime.Now,
                        //    BUser = historyData.BUser,
                        //    Reason = historyData.Reason
                        //};

                        //dbContext.TblRFIDHistory.Add(historyRecord);
                        //await dbContext.SaveChangesAsync();
                    }



                    //if (clients.TryGetValue(recipientId, out var recipientSocket) && recipientSocket.State == WebSocketState.Open)
                    //{
                    //    await recipientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseMessage)), WebSocketMessageType.Text, true, CancellationToken.None);
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"Client with ID {recipientId} not found or not connected.");
                    //}

                }
                else if (type == "Closing") {
                    var closingResponse = JsonSerializer.Serialize(new { Type = "closed", ClientId = clientId });
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(closingResponse)), WebSocketMessageType.Text, true, CancellationToken.None);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested to close the connection", CancellationToken.None);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    //finally
    //{
    //    if (webSocket.State == WebSocketState.Open)
    //    {
    //        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    //    }
    //    clients.TryRemove(clientId, out _);
    //    Console.WriteLine($"Client with ID {clientId} disconnected.");
    //    webSocket.Dispose();
    //}
}
