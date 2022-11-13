using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Application
{
    public class GameClient
    {
        private readonly Bot _bot;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public static void Run()
        {
            new GameClient()
                .StartGameClient()
                .Wait();
        }

        private GameClient()
        {
            _bot = new Bot();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };

            _jsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        private async Task StartGameClient(string address = "127.0.0.1:8765")
        {
            using (var webSocket = new ClientWebSocket())
            {
                var serverUri = new Uri("ws://" + address);
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);

                var token = Environment.GetEnvironmentVariable("TOKEN");
                var registerPayload = token == null
                    ? JsonConvert.SerializeObject(new { type = "REGISTER", teamName = Bot.NAME })
                    : JsonConvert.SerializeObject(new { type = "REGISTER", token = token });


                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(registerPayload)),
                    WebSocketMessageType.Text, true, CancellationToken.None);

                while (webSocket.State == WebSocketState.Open)
                {
                    var message = await ReadMessage(webSocket);
                    var tick = JsonConvert.DeserializeObject<Tick>(message, _jsonSerializerSettings);
                    if (tick != null)
                    {
                        Console.WriteLine("Playing tick " + tick.CurrentTick + " of " + tick.TotalTicks);
                        var serializedCommand = JsonConvert.SerializeObject(new
                            {
                                type = "COMMAND",
                                action = _bot.GetNextMove(tick),
                                tick = tick.CurrentTick
                            },
                            _jsonSerializerSettings);

                        await webSocket.SendAsync(
                            Encoding.UTF8.GetBytes(serializedCommand),
                            WebSocketMessageType.Text,
                            true, CancellationToken.None);
                    }
                }
            }
        }

        private static async Task<string> ReadMessage(ClientWebSocket client)
        {
            var result = "";
            WebSocketReceiveResult receiveResult;
            do
            {
                var segment = new ArraySegment<byte>(new byte[1024]);
                receiveResult = await client.ReceiveAsync(segment, CancellationToken.None);
                result += Encoding.UTF8.GetString(segment.Array);
            } while (!receiveResult.EndOfMessage);

            return result;
        }
    }
}