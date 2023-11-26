using System.Text.Json.Serialization;
using Dapr;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {app.UseDeveloperExceptionPage();}

string DAPR_STORE_NAME = "cosmosdb";
var client = new DaprClientBuilder().Build();

// Dapr subscription in [Topic] routes orders topic to this route

app.MapPost("/orders", [Topic("servicebus", "orders")]
async (Order order) => {
    Console.WriteLine("Subscriber received : " + order);
    int orderId = order.OrderId;

//Save state into the state store

    await client.SaveStateAsync(DAPR_STORE_NAME, orderId.ToString(), order.ToString());
    Console.WriteLine("Saving Order: " + order);
    return Results.Ok(order);
});

await app.RunAsync();

public record Order([property: JsonPropertyName("orderId")] int OrderId);
